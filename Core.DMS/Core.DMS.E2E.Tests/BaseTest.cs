using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using System;

namespace Core.DMS.E2E.Tests
{
    public class BaseTest
    {
        public int AppId { get; set; }
        public string UserId { get; set; } = "test.user";

        private string databaseName;
        public SqlTransaction Transaction;
        public SqlConnection Connection;

        public void Setup()
        {
            databaseName = $"cCoderCore-{DateTimeOffset.Now.Ticks}";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string connectionString = configuration.GetConnectionString("TestServer");

            Connection = new SqlConnection(connectionString);

            var svc = new DacServices(connectionString);

            var deploymentOptions = new DacDeployOptions
            {
                CreateNewDatabase = true,
            };

            svc.Deploy(DacPackage.Load(System.IO.Directory.GetCurrentDirectory() + "/cCoderCore-DMS-Module.dacpac"), databaseName, true, deploymentOptions);

            Connection.Open();
            Transaction = Connection.BeginTransaction();

            Connection.Execute($@"USE [{databaseName}]", transaction: Transaction);

            AppId = Connection.ExecuteScalar<int>(
                sql: "INSERT INTO [CMS].[Apps] ([Domain]) VALUES (''); SELECT SCOPE_IDENTITY();",
                transaction: Transaction);

        }

        public void Cleanup()
        {
            Transaction.Rollback();

            Connection.Execute($@"USE [master];
ALTER DATABASE [{databaseName}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{databaseName}];
EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{databaseName}';");
            Connection.Close();
        }
    }
}
