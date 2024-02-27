using Core.DMS.Objects.DTOs;
using Core.DMS.Objects.Entities;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Core.DMS.Data.Brokers
{
    public class FileContentBroker : IFileContentBroker
    {
        private readonly IDbConnection connection;

        public FileContentBroker(IDbConnection connection)
        {
            this.connection = connection;
        }

        public Task<FileContentView> GetFileContents(int appId, string userId, string path, int version)
            => connection.QueryFirstOrDefaultAsync<FileContentView>(
                sql: "[DMS].[GetFileContents]",
                param: new { AppId = appId, UserId = userId, Path = path, Version = version },
                commandType: CommandType.StoredProcedure
            );

        public Task<IEnumerable<FileContentView>> GetAllFileContents(int appId, string userId, string startingPath)
            => connection.QueryAsync<FileContentView>(
                sql: "[DMS].[GetAllFileContents]",
                param: new { AppId = appId, UserId = userId, StartingPath = startingPath },
                commandType: CommandType.StoredProcedure
            );

        public Task CreateFileContent(FileContent fileContent)
            => connection.ExecuteAsync(
                sql: "[DMS].[CreateFileVersion]",
                param: new { UserId = fileContent.CreatedBy, fileContent.FileId, fileContent.Size, fileContent.RawData });
    }
}
