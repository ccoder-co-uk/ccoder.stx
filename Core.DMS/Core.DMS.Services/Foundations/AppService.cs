using Core.DMS.Data.Brokers;

namespace Core.DMS.Services.Foundations
{
    public class AppService : IAppService
    {
        private readonly IAppBroker broker;
        private int appId = 0;

        public AppService(IAppBroker broker)
        {
            this.broker = broker;
        }

        public int GetAppId()
        {
            if (appId == 0)
                appId = broker.GetAppId();

            return appId;
        }
    }
}
