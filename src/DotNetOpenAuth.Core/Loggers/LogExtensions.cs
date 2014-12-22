namespace DotNetOpenAuth.Loggers {
    using System;
    using System.Globalization;

    public static class LogExtensions {
        public static bool IsDebugEnabled(this ILog logger) {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Debug, null);
        }

        public static bool IsErrorEnabled(this ILog logger) {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Error, null);
        }

        public static bool IsFatalEnabled(this ILog logger) {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Fatal, null);
        }

        public static bool IsInfoEnabled(this ILog logger) {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Info, null);
        }

        public static bool IsTraceEnabled(this ILog logger) {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Trace, null);
        }

        public static bool IsWarnEnabled(this ILog logger) {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Warn, null);
        }

        public static void Debug(this ILog logger, Func<string> messageFunc) {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Debug, messageFunc);
        }

        public static void Debug(this ILog logger, string message) {
            if (logger.IsDebugEnabled()) {
                logger.Log(LogLevel.Debug, message.AsFunc());
            }
        }

        public static void DebugFormat(this ILog logger, string message, params object[] args) {
            if (logger.IsDebugEnabled()) {
                logger.LogFormat(LogLevel.Debug, message, args);
            }
        }

        public static void DebugException(this ILog logger, string message, Exception exception) {
            if (logger.IsDebugEnabled()) {
                logger.Log(LogLevel.Debug, message.AsFunc(), exception);
            }
        }

        public static void Error(this ILog logger, Func<string> messageFunc) {
            logger.Log(LogLevel.Error, messageFunc);
        }

        public static void Error(this ILog logger, string message) {
            if (logger.IsErrorEnabled()) {
                logger.Log(LogLevel.Error, message.AsFunc());
            }
        }

        public static void ErrorFormat(this ILog logger, string message, params object[] args) {
            if (logger.IsErrorEnabled()) {
                logger.LogFormat(LogLevel.Error, message, args);
            }
        }

        public static void ErrorException(this ILog logger, string message, Exception exception) {
            if (logger.IsErrorEnabled()) {
                logger.Log(LogLevel.Error, message.AsFunc(), exception);
            }
        }

        public static void Fatal(this ILog logger, Func<string> messageFunc) {
            logger.Log(LogLevel.Fatal, messageFunc);
        }

        public static void Fatal(this ILog logger, string message) {
            if (logger.IsFatalEnabled()) {
                logger.Log(LogLevel.Fatal, message.AsFunc());
            }
        }

        public static void FatalFormat(this ILog logger, string message, params object[] args) {
            if (logger.IsFatalEnabled()) {
                logger.LogFormat(LogLevel.Fatal, message, args);
            }
        }

        public static void FatalException(this ILog logger, string message, Exception exception) {
            if (logger.IsFatalEnabled()) {
                logger.Log(LogLevel.Fatal, message.AsFunc(), exception);
            }
        }

        public static void Info(this ILog logger, Func<string> messageFunc) {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Info, messageFunc);
        }

        public static void Info(this ILog logger, string message) {
            if (logger.IsInfoEnabled()) {
                logger.Log(LogLevel.Info, message.AsFunc());
            }
        }

        public static void InfoFormat(this ILog logger, string message, params object[] args) {
            if (logger.IsInfoEnabled()) {
                logger.LogFormat(LogLevel.Info, message, args);
            }
        }

        public static void InfoException(this ILog logger, string message, Exception exception) {
            if (logger.IsInfoEnabled()) {
                logger.Log(LogLevel.Info, message.AsFunc(), exception);
            }
        }

        public static void Trace(this ILog logger, Func<string> messageFunc) {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Trace, messageFunc);
        }

        public static void Trace(this ILog logger, string message) {
            if (logger.IsTraceEnabled()) {
                logger.Log(LogLevel.Trace, message.AsFunc());
            }
        }

        public static void TraceFormat(this ILog logger, string message, params object[] args) {
            if (logger.IsTraceEnabled()) {
                logger.LogFormat(LogLevel.Trace, message, args);
            }
        }

        public static void TraceException(this ILog logger, string message, Exception exception) {
            if (logger.IsTraceEnabled()) {
                logger.Log(LogLevel.Trace, message.AsFunc(), exception);
            }
        }

        public static void Warn(this ILog logger, Func<string> messageFunc) {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, messageFunc);
        }

        public static void Warn(this ILog logger, string message) {
            if (logger.IsWarnEnabled()) {
                logger.Log(LogLevel.Warn, message.AsFunc());
            }
        }

        public static void WarnFormat(this ILog logger, string message, params object[] args) {
            if (logger.IsWarnEnabled()) {
                logger.LogFormat(LogLevel.Warn, message, args);
            }
        }

        public static void WarnException(this ILog logger, string message, Exception exception) {
            if (logger.IsWarnEnabled()) {
                logger.Log(LogLevel.Warn, message.AsFunc(), exception);
            }
        }

        private static void GuardAgainstNullLogger(ILog logger) {
            if (logger == null) {
                throw new ArgumentNullException("logger");
            }
        }

        private static void LogFormat(this ILog logger, LogLevel logLevel, string message, params object[] args) {
            var result = string.Format(CultureInfo.InvariantCulture, message, args);
            logger.Log(logLevel, result.AsFunc());
        }

        // Avoid the closure allocation, see https://gist.github.com/AArnott/d285feef75c18f6ecd2b
        private static Func<T> AsFunc<T>(this T value) where T : class {
            return value.Return;
        }

        private static T Return<T>(this T value) {
            return value;
        }
    }
}