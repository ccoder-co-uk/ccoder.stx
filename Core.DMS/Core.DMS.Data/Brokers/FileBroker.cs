using Core.DMS.Objects.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Core.DMS.Data.Brokers
{
    public class FileBroker : IFileBroker
    {
        private readonly IDbConnection connection;

        public FileBroker(IDbConnection connection)
        {
            this.connection = connection;
        }

        public Task<int> GetFileCountInFolder(int appId, string userId, Guid folderId)
            => connection.ExecuteScalarAsync<int>(
                sql: "[DMS].[GetFileCountInFolder]",
                param: new { AppId = appId, UserId = userId, Folder = folderId },
                commandType: CommandType.StoredProcedure
            );

        public Task<IEnumerable<File>> GetFilesInFolder(int appId, string userId, Guid folderId, int skip = 0, int take = 100)
            => connection.QueryAsync<File>(
                sql: "[DMS].[GetFilesForFolderId]",
                param: new { AppId = appId, UserId = userId, FolderId = folderId, Skip = skip, Take = take },
                commandType: CommandType.StoredProcedure
            );

        public Task<IEnumerable<File>> GetFiles(int appId, string userId, string startingPath)
            => connection.QueryAsync<File>(
                sql: "[DMS].[GetAllFiles]",
                param: new { AppId = appId, UserId = userId, StartingPath = startingPath },
                commandType: CommandType.StoredProcedure
            );

        public Task<File> GetFile(int appId, string userId, string path)
            => connection.QueryFirstOrDefaultAsync<File>(
                sql: "[DMS].[GetFile]",
                param: new { AppId = appId, UserId = userId, Path = path },
                commandType: CommandType.StoredProcedure
            );

        public Task<bool> CanCreateFile(int appId, string userId, string folderPath)
            => connection.ExecuteScalarAsync<bool>(
                    sql: "SELECT [DMS].[CanCreateFile] (@UserId, @AppId, @FolderPath)",
                    param: new { UserId = userId, AppId = appId, FolderPath = folderPath },
                    commandType: CommandType.Text);

        public Task<bool> CanUpdateFile(int appId, string userId, string folderPath)
            => connection.ExecuteScalarAsync<bool>(
                    sql: "SELECT [DMS].[CanUpdateFile] (@UserId, @Domain, @FolderPath)",
                    param: new { UserId = userId, AppId = appId, FolderPath = folderPath },
                    commandType: CommandType.Text);

        public Task<bool> FileExists(int appId, string userId, string path)
            => connection.ExecuteScalarAsync<bool>(
                sql: "SELECT [DMS].[CanUpdateFile] (@UserId, @Domain, @Path)",
                param: new { UserId = userId, AppId = appId, Path = path },
                commandType: CommandType.Text);

        public Task<int> CreateFile(File file, string folderPath, int appId, byte[] rawData)
            => connection.ExecuteAsync(
                sql: "[DMS].[CreateFile]",
                param: new { UserId = file.CreatedBy, file.Name, FolderPath = folderPath, AppId = appId, file.MimeType, file.Size, RawData = rawData },
                commandType: CommandType.StoredProcedure);

        public Task MoveFileFromFolderAndCreateFileAtDestination(int appId, string userId, string oldPath, string newPath)
            => connection.ExecuteAsync(
                sql: "[DMS].[MoveFileToFolderAndCreateFile]",
                param: new { UserId = userId, AppId = appId, OldPath = oldPath, NewPath = newPath },
                commandType: CommandType.StoredProcedure);

        public Task MoveFileFromFolderAndUpdateFileAtDestination(int appId, string userId, string oldPath, string newPath)
            => connection.ExecuteAsync(
                sql: "[DMS].[MoveFileToFolderAndUpdateFile]",
                param: new { UserId = userId, AppId = appId, OldPath = oldPath, NewPath = newPath },
                commandType: CommandType.StoredProcedure);
    }
}
