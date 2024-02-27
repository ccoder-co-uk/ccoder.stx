using Microsoft.Extensions.Logging;

namespace Core.DMS.Data.Brokers
{
    public class LoggingBroker : ILoggingBroker
    {
        private readonly ILoggerFactory loggerFactory;

        public LoggingBroker(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public void LogDebug<T>(string message) where T : class
            => loggerFactory.CreateLogger(typeof(T).FullName).LogDebug(message);

        public void LogInfo<T>(string message) where T : class
            => loggerFactory.CreateLogger(typeof(T).FullName).LogInformation(message);

        public void LogError<T>(string message) where T : class
            => loggerFactory.CreateLogger(typeof(T).FullName).LogError(message);

        public void LogWarning<T>(string message) where T : class
            => loggerFactory.CreateLogger(typeof(T).FullName).LogWarning(message);
    }
}
