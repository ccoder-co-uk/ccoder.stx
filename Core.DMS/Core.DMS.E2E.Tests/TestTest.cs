using Core.DMS.E2E.Tests.Brokers;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using System;
using Xunit;

namespace Core.DMS.E2E.Tests
{
    public class TestTest
    {
        [Fact]
        public void RunStoredProcedureTests()
        {
            //given
            string databaseName = $"cCoderCore-{DateTimeOffset.Now.Ticks}";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = configuration.GetConnectionString("TestServer");

            using var sqlConnection = new SqlConnection(connectionString);
         
            var svc = new DacServices(connectionString);

            svc.Deploy(DacPackage.Load(System.IO.Directory.GetCurrentDirectory() + "/cCoder-Core-DMS.dacpac"), databaseName, true);

            //when


            //then
            sqlConnection.Execute($@"USE [master]
GO
ALTER DATABASE [{databaseName}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

GO
USE [master]
GO
DROP DATABASE [{databaseName}]
GO
EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{databaseName}'
GO");
        }
    }
}
