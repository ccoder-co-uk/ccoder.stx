namespace Core.DMS.Data.Brokers
{
    public interface ILoggingBroker
    {
        void LogDebug<T>(string message) where T : class;
        void LogError<T>(string message) where T : class;
        void LogInfo<T>(string message) where T : class;
        void LogWarning<T>(string message) where T : class;
    }
}