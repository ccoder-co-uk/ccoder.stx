using Core.DMS.Data.Brokers;
using Core.DMS.Objects.DTOs;
using Core.DMS.Objects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.DMS.Services.Foundations
{
    public class FileContentService(IFileContentBroker fileContentBroker) : IFileContentService
    {
        public Task<FileContentView> GetFileContents(int appId, string userId, string path, int version = 0)
            => fileContentBroker.GetFileContents(appId, userId, path, version);

        public Task<IEnumerable<FileContentView>> GetAllFileContents(int appId, string userId, string startingPath)
            => fileContentBroker.GetAllFileContents(appId, userId, startingPath);

        public Task CreateFileContent(FileContent fileContent)
            => fileContentBroker.CreateFileContent(fileContent);
    }
}
