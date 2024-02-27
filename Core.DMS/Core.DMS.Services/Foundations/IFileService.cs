using Core.DMS.Objects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.DMS.Services.Foundations
{
    public interface IFileService
    {
        Task<IEnumerable<File>> GetFiles(int appId, string userId, string path);
        Task<File> GetFile(int appId, string userId, string path);
        Task<bool> CanCreateFile(int appId, string userId, string path);
        Task<bool> CanUpdateFile(int appId, string userId, string path);
        Task<int> CreateFile(File file, string folderPath, int appId, byte[] rawData);
        Task<bool> FileExists(int appId, string userId, string path);
        Task MoveFileFromFolderAndCreateFileAtDestination(int appId, string userId, string oldPath, string newPath);
        Task MoveFileFromFolderAndUpdateFileAtDestination(int appId, string userId, string oldPath, string newPath);
    }
}
