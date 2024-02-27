using Core.DMS.Objects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.DMS.Services.Foundations
{
    public interface IFolderService
    {
        Task<IEnumerable<Folder>> GetAllFolders(int appId, string userId, string startingPath);
        Task<Folder> GetFolder(int appId, string userId, string path);
        Task<bool> CanBuildFolderAtPath(int appId, string userId, string path);
        Task BuildFolderPath(int appId, string path);
        Task<bool> CanMoveFolderAndChildren(int appId, string userId, string folderPath);
        Task<bool> FolderExists(int appId, string userId, string path);
        Task<bool> CanCreateFolder(int appId, string userId, string path);
        Task<bool> HasPrivToMoveFolderToExistingFolder(int appId, string userId, string oldPath, string newPath);
        Task MoveFolderToFolder(int appId, string userId, string oldPath, string newPath);
    }
}