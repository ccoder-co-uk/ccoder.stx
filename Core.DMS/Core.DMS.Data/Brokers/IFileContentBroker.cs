using Core.DMS.Objects.DTOs;
using Core.DMS.Objects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.DMS.Data.Brokers
{
    public interface IFileContentBroker
    {
        Task<FileContentView> GetFileContents(int appId, string userId, string path, int version = 0);
        Task<IEnumerable<FileContentView>> GetAllFileContents(int appId, string userId, string startingPath);
        Task CreateFileContent(FileContent fileContent);
    }
}