using Core.DMS.Data.Brokers;
using Core.DMS.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.DMS.Services.Foundations
{
    public class FileService : IFileService
    {
        private readonly IFileBroker fileBroker;

        public FileService(IFileBroker fileBroker)
        {
            this.fileBroker = fileBroker;
        }

        public Task<IEnumerable<File>> GetFilesInFolder(int appId, string userId, Guid folderId, int skip = 0, int take = 100)
            => fileBroker.GetFilesInFolder(appId, userId, folderId, skip, take);

        public Task<int> GetFileCountInFolder(int appId, string userId, Guid folderId)
            => fileBroker.GetFileCountInFolder(appId, userId, folderId);

        public Task<IEnumerable<File>> GetFiles(int appId, string userId, string startingPath)
            => fileBroker.GetFiles(appId, userId, startingPath);

        public Task<File> GetFile(int appId, string userId, string path)
            => fileBroker.GetFile(appId, userId, path);

        public Task<bool> CanCreateFile(int appId, string userId, string path)
            => fileBroker.CanCreateFile(appId, userId, path);

        public Task<bool> CanUpdateFile(int appId, string userId, string path)
            => fileBroker.CanUpdateFile(appId, userId, path);

        public Task<int> CreateFile(File file, string folderPath, int appId, byte[] rawData)
            => fileBroker.CreateFile(file, folderPath, appId, rawData);

        public Task<bool> FileExists(int appId, string userId, string path)
            => fileBroker.FileExists(appId, userId, path);

        public Task MoveFileFromFolderAndCreateFileAtDestination(int appId, string userId, string oldPath, string newPath)
            => fileBroker.MoveFileFromFolderAndCreateFileAtDestination(appId, userId, oldPath, newPath);

        public Task MoveFileFromFolderAndUpdateFileAtDestination(int appId, string userId, string oldPath, string newPath)
            => fileBroker.MoveFileFromFolderAndUpdateFileAtDestination(appId, userId, oldPath, newPath);
    }
}
