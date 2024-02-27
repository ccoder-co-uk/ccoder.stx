using Core.DMS.Objects.DTOs;
using Core.DMS.Objects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.DMS.Services.Foundations
{
    public interface IFileContentService
    {
        Task<IEnumerable<FileContentView>> GetAllFileContents(int appId, string userId, string startingPath);
        Task<FileContentView> GetFileContents(int appId, string userId, string path, int version = 0);
        Task CreateFileContent(FileContent fileContent);
    }
}