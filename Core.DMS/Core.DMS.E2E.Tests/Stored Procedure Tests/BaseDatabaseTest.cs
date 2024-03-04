using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using System;

namespace Core.DMS.E2E.Tests
{
    public class BaseDatabaseTest
    {
        public int AppId { get; set; }
        public string UserId { get; set; } = "test.user";

        public string DatabaseName { get; set; }

        public SqlTransaction Transaction;
        public SqlConnection Connection;
        public IConfiguration Configuration;

        public void SetupDatabase()
        {
            DatabaseName = $"cCoderCore-{DateTimeOffset.Now.Ticks}";

            Configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string connectionString = Configuration.GetConnectionString("TestServer");

            Connection = new SqlConnection(connectionString);

            var svc = new DacServices(connectionString);

            var deploymentOptions = new DacDeployOptions
            {
                CreateNewDatabase = true,
            };

            svc.Deploy(DacPackage.Load(System.IO.Directory.GetCurrentDirectory() + "/cCoderCore-DMS-Module.dacpac"), DatabaseName, true, deploymentOptions);

            Connection.Open();
            Transaction = Connection.BeginTransaction();

            Connection.Execute($@"USE [{DatabaseName}]", transaction: Transaction);

            Connection.Execute($"INSERT INTO [Security].[Users] ([Id]) VALUES ('{UserId}')", transaction: Transaction);

            AppId = Connection.ExecuteScalar<int>(
                sql: "INSERT INTO [CMS].[Apps] ([Domain]) VALUES ('localhost'); SELECT SCOPE_IDENTITY();",
                transaction: Transaction);

        }

        public void CleanupDatabase()
        {
            if (Transaction != null)
                Transaction.Rollback();

            Connection.Execute($@"USE [master];
ALTER DATABASE [{DatabaseName}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{DatabaseName}];
EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{DatabaseName}';");
            Connection.Close();
        }
    }
}
