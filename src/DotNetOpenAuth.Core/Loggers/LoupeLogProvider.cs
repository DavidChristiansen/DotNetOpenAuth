namespace DotNetOpenAuth.Loggers {
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public class LoupeLogProvider : ILogProvider {
        private static bool _providerIsAvailableOverride = true;
        private readonly WriteDelegate _logWriteDelegate;

        public LoupeLogProvider() {
            if (!IsLoggerAvailable()) {
                throw new InvalidOperationException("Gibraltar.Agent.Log (Loupe) not found");
            }

            _logWriteDelegate = GetLogWriteDelegate();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [provider is available override]. Used in tests.
        /// </summary>
        /// <value>
        /// <c>true</c> if [provider is available override]; otherwise, <c>false</c>.
        /// </value>
        public static bool ProviderIsAvailableOverride {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name) {
            return new LoupeLogger(name, _logWriteDelegate);
        }

        public static bool IsLoggerAvailable() {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetLogManagerType() {
            return Type.GetType("Gibraltar.Agent.Log, Gibraltar.Agent");
        }

        private static WriteDelegate GetLogWriteDelegate() {
            Type logManagerType = GetLogManagerType();
            Type logMessageSeverityType = Type.GetType("Gibraltar.Agent.LogMessageSeverity, Gibraltar.Agent");
            Type logWriteModeType = Type.GetType("Gibraltar.Agent.LogWriteMode, Gibraltar.Agent");

            MethodInfo method = logManagerType.GetMethod("Write", new[]
            {
                logMessageSeverityType, typeof(string), typeof(int), typeof(Exception), typeof(bool), 
                logWriteModeType, typeof(string), typeof(string), typeof(string), typeof(string), typeof(object[])
            });

            var callDelegate = (WriteDelegate)Delegate.CreateDelegate(typeof(WriteDelegate), method);
            return callDelegate;
        }

        public class LoupeLogger : ILog {
            private const string LogSystem = "LibLog";

            private readonly string _category;
            private readonly WriteDelegate _logWriteDelegate;
            private readonly int _skipLevel;

            internal LoupeLogger(string category, WriteDelegate logWriteDelegate) {
                _category = category;
                _logWriteDelegate = logWriteDelegate;
                _skipLevel = 1;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
                if (messageFunc == null) {
                    //nothing to log..
                    return true;
                }

                _logWriteDelegate((int)ToLogMessageSeverity(logLevel), LogSystem, _skipLevel, exception, true, 0, null,
                    _category, null, messageFunc.Invoke());

                return true;
            }

            public TraceEventType ToLogMessageSeverity(LogLevel logLevel) {
                switch (logLevel) {
                    case LogLevel.Trace:
                        return TraceEventType.Verbose;
                    case LogLevel.Debug:
                        return TraceEventType.Verbose;
                    case LogLevel.Info:
                        return TraceEventType.Information;
                    case LogLevel.Warn:
                        return TraceEventType.Warning;
                    case LogLevel.Error:
                        return TraceEventType.Error;
                    case LogLevel.Fatal:
                        return TraceEventType.Critical;
                    default:
                        throw new ArgumentOutOfRangeException("logLevel");
                }
            }
        }

        /// <summary>
        /// The form of the Loupe Log.Write method we're using
        /// </summary>
        internal delegate void WriteDelegate(
            int severity,
            string logSystem,
            int skipFrames,
            Exception exception,
            bool attributeToException,
            int writeMode,
            string detailsXml,
            string category,
            string caption,
            string description,
            params object[] args
            );
    }
}