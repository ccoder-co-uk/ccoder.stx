using Core.DMS.Data.Brokers;
using Core.DMS.Objects.DTOs;
using Core.DMS.Objects.Entities;
using Core.DMS.Services.Foundations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Path = Core.DMS.Objects.Path;
using File = Core.DMS.Objects.Entities.File;
using System;

namespace Core.DMS.Services.Orchestration
{
    public class DMSResultOrchestrationService(IFolderService folderService,
        IFileService fileService,
        IFileContentService fileContentService,
        IAuthenticationBroker authenticationBroker,
        IAppService appService,
        ILoggingBroker loggingBroker) : IDMSResultOrchestrationService
    {
        public async Task Save(Path path, MemoryStream content = null)
        {
            int appId = appService.GetAppId();
            string ssoUserId = authenticationBroker.GetUserId();

            if (path.IsToFile)
            {
                byte[] requestBytes = content?.ToArray() ?? Array.Empty<byte>();

                File existingFile = await fileService.GetFile(appId, ssoUserId, path.Lowered);

                if (existingFile == null)
                {
                    bool existingFileWithoutAccessCheck = await fileService.FileExists(appId, ssoUserId, path.Lowered);

                    if (!existingFileWithoutAccessCheck)
                    {
                        loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} unable to read file at path {path.Lowered}");
                        throw new SecurityException("Access Denied!");
                    }

                    await BuildPath(path.ParentPath, ssoUserId, appId);

                    await CreateNewFile(path, requestBytes, path.ParentPath.FullPath, ssoUserId, appId);
                }
                else
                    await UpdateFile(existingFile, requestBytes, ssoUserId, path.ParentPath.FullPath, appId);
            }
            else
                await BuildPath(path, ssoUserId, appId);
        }

        private async Task BuildPath(Path folderPath, string ssoUserId, int appId)
        {
            var hasAccess = await folderService.CanBuildFolderAtPath(appId, ssoUserId, folderPath.Lowered);

            if (!hasAccess)
            {
                loggingBroker.LogWarning<DMSResultOrchestrationService>($"User can't build path {folderPath} in DMS for app {appService.GetAppId()}");
                throw new SecurityException("Access Denied!");
            }

            await folderService.BuildFolderPath(appId, folderPath.Lowered);
        }

        private async Task CreateNewFile(Path path, byte[] requestBytes, string folderPath, string ssoUserId, int appId)
        {
            //Check user has file_create within folder

            bool hasAccess = await fileService.CanCreateFile(appId, ssoUserId, folderPath);

            if (!hasAccess)
            {
                loggingBroker.LogWarning<DMSResultOrchestrationService>($"User can't create a file in folder {folderPath} in DMS for app {appId}");
                throw new SecurityException("Access Denied!");
            }
            
            await CreateFile(path, requestBytes, folderPath, ssoUserId, appId);
        }

        private async Task CreateFile(Path path, byte[] requestBytes, string folderPath, string ssoUserId, int appId)
        {
            File fileObject = new()
            {
                CreatedBy = ssoUserId,
                CreatedOn = DateTimeOffset.UtcNow,
                Name = path.Name.ToLower(),
                Path = path.Lowered,
                MimeType = path.MimeType,
                Size = GetSizeOf(requestBytes),
            };

            await fileService.CreateFile(fileObject, folderPath, appId, requestBytes);
        }

        static string GetSizeOf(byte[] content)
        {
            if (content.Length > 1000000000)
            {
                return $"{content.Length / 1000 / 1000 / 1000} GB";
            }

            if (content.Length > 1000000)
            {
                return $"{content.Length / 1000 / 1000} MB";
            }

            return content.Length > 1000
                ? $"{content.Length / 1000} KB"
                : $"{content.Length} B";
        }

        private async Task UpdateFile(File existingFile, byte[] rawBytes, string ssoUserId, string folderPath, int appId)
        {
            bool hasAccess = await fileService.CanUpdateFile(appId, ssoUserId, folderPath);

            if (!hasAccess)
            {
                loggingBroker.LogWarning<DMSResultOrchestrationService>($"User can't create a file in folder {folderPath} in DMS for app {appId}");
                throw new SecurityException("Access Denied!");
            }

            await fileContentService.CreateFileContent(new FileContent
            {
                CreatedBy = ssoUserId,
                CreatedOn = DateTimeOffset.UtcNow,
                FileId = existingFile.Id,
                Size = GetSizeOf(rawBytes),
                RawData = rawBytes
            });
        }

        public async Task Move(Path oldPath, Path newPath)
        {
            string ssoUserId = authenticationBroker.GetUserId();
            int appId = appService.GetAppId();

            if (oldPath.IsToFolder && newPath.IsToFolder)
            {
                //for content?moveTo=content/foo scenarios
                if (newPath.FullPath.StartsWith(oldPath.FullPath))
                    throw new InvalidOperationException("You cannot move a parent folder into a child folder of the parent");

                //Check user can folder_update at oldPath and it's subfolders.
                bool canMoveFolderAndChildren = await folderService.CanMoveFolderAndChildren(appId, ssoUserId, oldPath.Lowered);

                if (!canMoveFolderAndChildren)
                {
                    loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} unable to update a folder at path {oldPath.Lowered}");
                    throw new SecurityException("Access Denied!");
                }

                bool folderExistsAtDestination = await folderService.FolderExists(appId, ssoUserId, newPath.Lowered);

                //If folder doesn't exist at destination, check user has folder_create priv and that's all.
                if (!folderExistsAtDestination)
                {
                    //TODO: Add file_create check as well...
                    bool canCreateFolderInDestination = await folderService.CanCreateFolder(appId, ssoUserId, newPath.ParentPath.Lowered);

                    if (!canCreateFolderInDestination)
                    {
                        loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} unable to create a folder in {newPath.ParentPath.Lowered} which is parent of requested folder to create {newPath.Lowered}");
                        throw new SecurityException("Access Denied!");
                    }

                    bool canCreateFilesInDestination = await fileService.CanCreateFile(appId, ssoUserId, newPath.ParentPath.Lowered);

                    if (!canCreateFilesInDestination)
                    {
                        loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} unable to create a file in {newPath.ParentPath.Lowered} which is parent of requested folder to create {newPath.Lowered}");
                    }
                }
                //If folder does exist at destination
                //  build up new paths based on the old paths
                //  get existing folders based on new paths, then check for appropriate folder_update privilege
                //  get new folders based on new paths, then check for appropriate folder_create privilege
                else
                {
                    bool hasPriv = await folderService.HasPrivToMoveFolderToExistingFolder(appId, ssoUserId, oldPath.Lowered, newPath.Lowered);

                    if (!hasPriv)
                    {
                        loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} unable to create/update a folder in {newPath.Lowered} or it's subfolders when moving a folder and merging them together");
                        throw new SecurityException("Access Denied!");
                    }
                }

                //Move the old path folder under the new path folder...
                await folderService.MoveFolderToFolder(appId, ssoUserId, oldPath.Lowered, newPath.Lowered);
            } else if (oldPath.IsToFolder && newPath.IsToFile)
                throw new InvalidOperationException("Cannot move folder into a file!");
                //Move old path folder to folder of file.
            else if (oldPath.IsToFile && newPath.IsToFolder)
            {
                //Move file to new path folder

                //Check file exists at old path specified..
                await CheckUserCanMoveExistingFile(oldPath, ssoUserId, appId);

                //Check user can file_create/file_update in new folder depending on if file exists
                bool fileExistsAtNewPath = await fileService.FileExists(appId, ssoUserId, newPath.Lowered);

                if (!fileExistsAtNewPath)
                {
                    bool canCreateFileInNewFolder = await fileService.CanCreateFile(appId, ssoUserId, newPath.Lowered);

                    if (!canCreateFileInNewFolder)
                    {
                        loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} cannot create a file at path {newPath}");
                        throw new SecurityException("Access Denied!");
                    }
                } 
                else
                {
                    bool canUpdateFileInNewFolder = await fileService.CanUpdateFile(appId, ssoUserId, newPath.Lowered);

                    if (!canUpdateFileInNewFolder)
                    {
                        loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} cannot update a file at path {newPath}");
                        throw new SecurityException("Access Denied!");
                    }
                }

                Path newFilePath = new Path($"{newPath}/{oldPath.Name}");

                //Check if file exists at new path, if so we should get the latest version of the file at the old path and add it to the latest set of versions
                //in the new file and drop the old file

                if (fileExistsAtNewPath)
                    await fileService.MoveFileFromFolderAndUpdateFileAtDestination(appId, ssoUserId, oldPath.FullPath, newFilePath.FullPath);
                //If the file doesn't exist at new path, then grab latest file version of the and then we just add it to the DB directly..
                else
                    await fileService.MoveFileFromFolderAndCreateFileAtDestination(appId, ssoUserId, oldPath.FullPath, newFilePath.FullPath);

            } else if (oldPath.IsToFile && newPath.IsToFile)
            {
                //Move file to specified file path
                //Scenarios 3 & 4 are similar code wise but have subtle differences around pathing as indicated by newPath.ParentPath etc.

                await CheckUserCanMoveExistingFile(oldPath, ssoUserId, appId);

                //Check user can file_create/file_update at new path depending on if file exists
                bool fileExistsAtNewPath = await fileService.FileExists(appId, ssoUserId, newPath.Lowered);

                if (!fileExistsAtNewPath)
                {
                    bool canCreateFileInNewFolder = await fileService.CanCreateFile(appId, ssoUserId, newPath.ParentPath.Lowered);

                    if (!canCreateFileInNewFolder)
                    {
                        loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} cannot create a file at path {newPath}");
                        throw new SecurityException("Access Denied!");
                    }
                }
                else
                {
                    bool canUpdateFileInNewFolder = await fileService.CanUpdateFile(appId, ssoUserId, newPath.ParentPath.Lowered);

                    if (!canUpdateFileInNewFolder)
                    {
                        loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} cannot update a file at path {oldPath}");
                        throw new SecurityException("Access Denied!");
                    }
                }

                //Check if file exists at new path, if so we should get the latest version of the file at the old path and add it to the latest set of versions
                //in the new file and drop the old file

                if (fileExistsAtNewPath)
                    await fileService.MoveFileFromFolderAndUpdateFileAtDestination(appId, ssoUserId, oldPath.FullPath, newPath.FullPath);
                else
                    await fileService.MoveFileFromFolderAndCreateFileAtDestination(appId, ssoUserId, oldPath.FullPath, newPath.FullPath);
            }
        }

        private async Task CheckUserCanMoveExistingFile(Path existingFilePath, string ssoUserId, int appId)
        {
            //Check file exists at old path specified..
            bool fileExistsAtOldPath = await fileService.FileExists(appId, ssoUserId, existingFilePath.Lowered);

            if (!fileExistsAtOldPath)
            {
                loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} asked to move file at path {existingFilePath} that doesn't exist!");
                throw new SecurityException("Access Denied!");
            }

            //Check user can file_update for the existing file
            string oldPathFolderPath = existingFilePath.ParentPath.Lowered;

            var canUpdateFileAtOldPath = await fileService.CanUpdateFile(appId, ssoUserId, oldPathFolderPath);

            if (!canUpdateFileAtOldPath)
            {
                loggingBroker.LogWarning<DMSResultOrchestrationService>($"User {ssoUserId} cannot update file at path {existingFilePath}");
                throw new SecurityException("Access Denied!");
            }
        }

        public async Task<DMSResult> Get(Path path, int version = 0)
        {
            string ssoUserId = authenticationBroker.GetUserId();
            int appId = appService.GetAppId();

            if (path.IsToFile)
            {
                var fileContent = await fileContentService.GetFileContents(appId, ssoUserId, path.Lowered, version);

                if (fileContent == null)
                {   
                    loggingBroker.LogWarning<DMSResultOrchestrationService>($"User can't see a file @ path {path.Lowered} in DMS for app {appId}");
                    throw new SecurityException("Access Denied!");
                }
                else
                {
                    return new DMSResult
                    {
                        MimeType = fileContent.MimeType,
                        Data = new MemoryStream(fileContent.RawData)
                    };
                }
            }
            else
            {
                Folder folder = await folderService.GetFolder(appId, ssoUserId, path.Lowered);

                if (folder == null)
                {
                    loggingBroker.LogWarning<DMSResultOrchestrationService>($"User can't see a folder @ path {path.Lowered} in DMS for app {appId}");
                    throw new SecurityException("Access Denied!");
                }
                else
                {
                    var result = await GetZipArchive(appId, ssoUserId, path.Lowered);

                    return new DMSResult
                    {
                        MimeType = "application/zip",
                        Data = new MemoryStream(result)
                    };
                }
            }
        }

        private async Task<byte[]> GetZipArchive(int appId, string userId, string startingPath)
        {
            using MemoryStream result = new();
            using (ZipArchive zip = new(result, ZipArchiveMode.Create))
            {
                var fileContents = await fileContentService.GetAllFileContents(appId, userId, startingPath);

                foreach (var fileContent in fileContents.Where(f => f.RawData != null))
                {
                    using Stream s = zip.CreateEntry(fileContent.Path, CompressionLevel.Optimal).Open();
                    s.Write(fileContent.RawData, 0, fileContent.RawData.Length);
                }
            }

            return result.ToArray();
        }

        /*public async Task Unpack(Path path, Stream content, bool ignoreArchiveRoot = false)
        {
            string ssoUserId = authenticationBroker.GetUserId();
            int appId = appService.GetAppId();

            loggingBroker.LogInfo<DMSResultOrchestrationService>($"Unpacking archive to {path.FullPath}");
            
            await BuildPath(path, ssoUserId, appId);

            if (!(db.User.IsAdminOfApp(app.Id) || folder.UserCan(db.User, "file_create")))
            {
                log.LogWarning($"User can't create a file in {folder.Path.ToLower()} in DMS for app {app.Id}");
                throw new SecurityException("Access Denied!");
            }

            ZipArchive archive = new(content, ZipArchiveMode.Read);
            string destinationPath;

            var rootEntry = archive.Entries
                .OrderBy(e => e.FullName.Split('/').Length)
                .First();

            var ignoreSegment = rootEntry.FullName;

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                using Stream entryStream = entry.Open();

                destinationPath = ignoreArchiveRoot
                    ? $"{path.FullPath}/{entry.FullName}".Replace(ignoreSegment, "")
                    : $"{path.FullPath}/{entry.FullName}";

                if (path.Lowered != destinationPath.ToLower())
                {
                    log.LogInformation($"   Unpacking entry {entry.FullName} to {destinationPath}");
                    await Save(new Objects.Path(destinationPath), entryStream);
                }

                entryStream.Close();
            }
        }*/
    }
}
