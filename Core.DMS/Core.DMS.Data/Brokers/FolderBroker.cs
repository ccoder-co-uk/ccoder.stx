using Core.DMS.Objects.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Core.DMS.Data.Brokers
{
    public class FolderBroker : IFolderBroker
    {
        private readonly IDbConnection connection;

        public FolderBroker(IDbConnection connection)
        {
            this.connection = connection;
        }

        public Task<IEnumerable<Folder>> GetFoldersForParentId(int appId, string userId, Guid? parentId)
            => connection.QueryAsync<Folder>(
                sql: "[DMS].[GetFoldersForParentId]",
                param: new { AppId = appId, UserId = userId, ParentId = parentId },
                commandType: CommandType.StoredProcedure
            );

        public Task<IEnumerable<Folder>> GetAllFolders(int appId, string userId, string startingPath)
            => connection.QueryAsync<Folder>(
                sql: "[DMS].[GetAllFolders]",
                param: new { AppId = appId, UserId = userId, StartingPath = startingPath },
                commandType: CommandType.StoredProcedure
            );

        public Task<Folder> GetFolder(int appId, string userId, string path)
            => connection.QueryFirstOrDefaultAsync<Folder>(
                sql: "[DMS].[GetFolder]",
                param: new { AppId = appId, UserId = userId, Path = path },
                commandType: CommandType.StoredProcedure
            );

        public Task<bool> CanBuildFolderAtPath(int appId, string userId, string folderPath)
            => connection.ExecuteScalarAsync<bool>(
                sql: "SELECT [DMS].[CanBuildFolderPath] (@UserId, @AppId, @FolderPath)",
                param: new { UserId = userId, AppId = appId, FolderPath = folderPath },
                commandType: CommandType.Text
            );

        public Task<bool> CanMoveFolderAndChildren(int appId, string userId, string folderPath)
            => connection.ExecuteScalarAsync<bool>(
                sql: "SELECT [DMS].[CanMoveFolderAndChildren] (@UserId, @AppId, @FolderPath)",
                param: new { UserId = userId, Domain = appId, FolderPath = folderPath },
                commandType: CommandType.Text
            );

        public Task<bool> FolderExists(int appId, string userId, string path)
            => connection.ExecuteScalarAsync<bool>(
                sql: "SELECT [DMS].[FolderExists] (@UserId, @AppId, @Path)",
                param: new { UserId = userId, AppId = appId, Path = path },
                commandType: CommandType.Text
            );

        public Task<bool> CanCreateFolder(int appId, string userId, string path)
            => connection.ExecuteScalarAsync<bool>(
                sql: "SELECT [DMS].[FolderExists] (@UserId, @Domain, @Path)",
                param: new { UserId = userId, AppId = appId, Path = path },
                commandType: CommandType.Text
            );

        public Task<bool> HasPrivToMoveFolderToExistingFolder(int appId, string userId, string oldPath, string newPath)
            => connection.ExecuteScalarAsync<bool>(
                sql: "SELECT [DMS].[HasPrivToMoveFolderToExistingFolder] (@UserId, @Domain, @OldPath, @NewPath)",
                param: new { UserId = userId, AppId = appId, OldPath = oldPath, NewPath = newPath },
                commandType: CommandType.Text);

        public Task BuildFolderPath(int appId, string path)
            => connection.ExecuteAsync(
                sql: "[DMS].[BuildFolderPath]",
                param: new { AppId = appId, FolderPath = path },
                commandType: CommandType.StoredProcedure);

        public Task MoveFolderToFolder(int appId, string userId, string oldPath, string newPath)
            => connection.ExecuteAsync(
                sql: "[DMS].[MoveFolderToFolder]",
                param: new { UserId = userId, AppId = appId, OldPath = oldPath, NewPath = newPath });
    }
}
