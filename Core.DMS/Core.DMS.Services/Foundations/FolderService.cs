using Core.DMS.Data.Brokers;
using Core.DMS.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.DMS.Services.Foundations
{
    public class FolderService : IFolderService
    {
        private readonly IFolderBroker broker;

        public FolderService(IFolderBroker broker)
        {
            this.broker = broker;
        }

        public Task<IEnumerable<Folder>> GetFoldersForParentId(int appId, string userId, Guid? parentId)
            => broker.GetFoldersForParentId(appId, userId, parentId);

        public Task<IEnumerable<Folder>> GetAllFolders(int appId, string userId, string startingPath)
            => broker.GetAllFolders(appId, userId, startingPath);

        public Task<Folder> GetFolder(int appId, string userId, string path)
            => broker.GetFolder(appId, userId, path);

        public Task<bool> CanBuildFolderAtPath(int appId, string userId, string path)
            => broker.CanBuildFolderAtPath(appId, userId, path);

        public Task<bool> CanMoveFolderAndChildren(int appId, string userId, string folderPath)
            => broker.CanMoveFolderAndChildren(appId, userId, folderPath);

        public Task BuildFolderPath(int appId, string path)
            => broker.BuildFolderPath(appId, path);

        public Task<bool> FolderExists(int appId, string userId, string path)
            => broker.FolderExists(appId, userId, path);

        public Task<bool> CanCreateFolder(int appId, string userId, string path)
            => broker.CanCreateFolder(appId, userId, path);

        public Task<bool> HasPrivToMoveFolderToExistingFolder(int appId, string userId, string oldPath, string newPath)
            => broker.HasPrivToMoveFolderToExistingFolder(appId, userId, oldPath, newPath);

        public Task MoveFolderToFolder(int appId, string userId, string oldPath, string newPath)
            => broker.MoveFolderToFolder(appId, userId, oldPath, newPath);
    }
}
