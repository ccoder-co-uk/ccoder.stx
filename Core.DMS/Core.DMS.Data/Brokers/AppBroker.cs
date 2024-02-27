using Core.DMS.Objects.DTOs;
using Dapper;
using System.Data;
using System.Threading.Tasks;

namespace Core.DMS.Data.Brokers
{
    public class AppBroker(IDbConnection connection, AppView appView) : IAppBroker
    {
        private readonly IDbConnection connection = connection;

        public int GetAppId()
            => connection.ExecuteScalar<int>(
                sql: "SELECT [Id] FROM [CMS].[Apps] WHERE Domain=@Domain",
                param: new { appView.Domain },
                commandType: CommandType.Text);

    }
}
