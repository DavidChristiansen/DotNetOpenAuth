namespace DotNetOpenAuth.Loggers {
    using System;

    public class LoggerExecutionWrapper : ILog {
        private readonly ILog _logger;
        public const string FailedToGenerateLogMessage = "Failed to generate log message";

        public ILog WrappedLogger {
            get { return _logger; }
        }

        public LoggerExecutionWrapper(ILog logger) {
            _logger = logger;
        }

        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null) {
            if (messageFunc == null) {
                return _logger.Log(logLevel, null);
            }

            Func<string> wrappedMessageFunc = () => {
                try {
                    return messageFunc();
                } catch (Exception ex) {
                    Log(LogLevel.Error, () => FailedToGenerateLogMessage, ex);
                }
                return null;
            };
            return _logger.Log(logLevel, wrappedMessageFunc, exception);
        }
    }
}