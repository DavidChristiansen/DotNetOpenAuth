namespace DotNetOpenAuth.Loggers {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    public class EntLibLogProvider : ILogProvider {
        private const string TypeTemplate = "Microsoft.Practices.EnterpriseLibrary.Logging.{0}, Microsoft.Practices.EnterpriseLibrary.Logging";
        private static bool _providerIsAvailableOverride = true;
        private static readonly Type LogEntryType;
        private static readonly Type LoggerType;
        private static readonly Action<string, string, TraceEventType> WriteLogEntry;
        private static Func<string, TraceEventType, bool> ShouldLogEntry;

        static EntLibLogProvider() {
            LogEntryType = Type.GetType(string.Format(TypeTemplate, "LogEntry"));
            LoggerType = Type.GetType(string.Format(TypeTemplate, "Logger"));
            if (LogEntryType == null || LoggerType == null) {
                return;
            }
            WriteLogEntry = GetWriteLogEntry();
            ShouldLogEntry = GetShouldLogEntry();
        }

        public EntLibLogProvider() {
            if (!IsLoggerAvailable()) {
                throw new InvalidOperationException("Microsoft.Practices.EnterpriseLibrary.Logging.Logger not found");
            }
        }

        public static bool ProviderIsAvailableOverride {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name) {
            return new EntLibLogger(name, WriteLogEntry, ShouldLogEntry);
        }

        public static bool IsLoggerAvailable() {
            return ProviderIsAvailableOverride && LogEntryType != null;
        }

        private static Action<string, string, TraceEventType> GetWriteLogEntry() {
            // new LogEntry(...)
            var logNameParameter = Expression.Parameter(typeof(string), "logName");
            var messageParameter = Expression.Parameter(typeof(string), "message");
            var severityParameter = Expression.Parameter(typeof(TraceEventType), "severity");

            MemberInitExpression memberInit = GetWriteLogExpression(messageParameter, severityParameter, logNameParameter);

            //Logger.Write(new LogEntry(....));
            MethodInfo writeLogEntryMethod = LoggerType.GetMethod("Write", new[] { LogEntryType });
            var writeLogEntryExpression = Expression.Call(writeLogEntryMethod, memberInit);

            return Expression.Lambda<Action<string, string, TraceEventType>>(
                writeLogEntryExpression,
                logNameParameter,
                messageParameter,
                severityParameter).Compile();
        }

        private static Func<string, TraceEventType, bool> GetShouldLogEntry() {
            // new LogEntry(...)
            var logNameParameter = Expression.Parameter(typeof(string), "logName");
            var severityParameter = Expression.Parameter(typeof(TraceEventType), "severity");

            MemberInitExpression memberInit = GetWriteLogExpression(Expression.Constant("***dummy***"), severityParameter, logNameParameter);

            //Logger.Write(new LogEntry(....));
            MethodInfo writeLogEntryMethod = LoggerType.GetMethod("ShouldLog", new[] { LogEntryType });
            var writeLogEntryExpression = Expression.Call(writeLogEntryMethod, memberInit);

            return Expression.Lambda<Func<string, TraceEventType, bool>>(
                writeLogEntryExpression,
                logNameParameter,
                severityParameter).Compile();
        }

        private static MemberInitExpression GetWriteLogExpression(Expression message,
            ParameterExpression severityParameter, ParameterExpression logNameParameter) {
            var entryType = LogEntryType;
            MemberInitExpression memberInit = Expression.MemberInit(Expression.New(entryType), new MemberBinding[]
            {
                Expression.Bind(entryType.GetProperty("Message"), message),
                Expression.Bind(entryType.GetProperty("Severity"), severityParameter),
                Expression.Bind(entryType.GetProperty("TimeStamp"),
                    Expression.Property(null, typeof (DateTime).GetProperty("UtcNow"))),
                Expression.Bind(entryType.GetProperty("Categories"),
                    Expression.ListInit(
                        Expression.New(typeof (List<string>)),
                        typeof (List<string>).GetMethod("Add", new[] {typeof (string)}),
                        logNameParameter))
            });
            return memberInit;
        }

        public class EntLibLogger : ILog {
            private readonly string _loggerName;
            private readonly Action<string, string, TraceEventType> _writeLog;
            private readonly Func<string, TraceEventType, bool> _shouldLog;

            internal EntLibLogger(string loggerName, Action<string, string, TraceEventType> writeLog, Func<string, TraceEventType, bool> shouldLog) {
                _loggerName = loggerName;
                _writeLog = writeLog;
                _shouldLog = shouldLog;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
                var severity = MapSeverity(logLevel);
                if (messageFunc == null) {
                    return _shouldLog(_loggerName, severity);
                }
                if (exception != null) {
                    return LogException(logLevel, messageFunc, exception);
                }
                _writeLog(_loggerName, messageFunc(), severity);
                return true;
            }

            public bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
                var severity = MapSeverity(logLevel);
                var message = messageFunc() + Environment.NewLine + exception;
                _writeLog(_loggerName, message, severity);
                return true;
            }

            private static TraceEventType MapSeverity(LogLevel logLevel) {
                switch (logLevel) {
                    case LogLevel.Fatal:
                        return TraceEventType.Critical;
                    case LogLevel.Error:
                        return TraceEventType.Error;
                    case LogLevel.Warn:
                        return TraceEventType.Warning;
                    case LogLevel.Info:
                        return TraceEventType.Information;
                    default:
                        return TraceEventType.Verbose;
                }
            }
        }
    }
}