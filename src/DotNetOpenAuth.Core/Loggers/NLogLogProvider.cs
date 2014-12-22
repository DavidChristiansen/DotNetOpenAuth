namespace DotNetOpenAuth.Loggers {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public class NLogLogProvider : ILogProvider {
        private readonly Func<string, object> _getLoggerByNameDelegate;
        private static bool _providerIsAvailableOverride = true;

        public NLogLogProvider() {
            if (!IsLoggerAvailable()) {
                throw new InvalidOperationException("NLog.LogManager not found");
            }
            _getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name) {
            return new NLogLogger(_getLoggerByNameDelegate(name));
        }

        public static bool IsLoggerAvailable() {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetLogManagerType() {
            return Type.GetType("NLog.LogManager, NLog");
        }

        private static Func<string, object> GetGetLoggerMethodCall() {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethod("GetLogger", new[] { typeof(string) });
            ParameterExpression nameParam = Expression.Parameter(typeof(string), "name");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] { nameParam });
            return Expression.Lambda<Func<string, object>>(methodCall, new[] { nameParam }).Compile();
        }

        public class NLogLogger : ILog {
            private readonly dynamic _logger;

            internal NLogLogger(dynamic logger) {
                _logger = logger;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
                if (messageFunc == null) {
                    return IsLogLevelEnable(logLevel);
                }
                if (exception != null) {
                    return LogException(logLevel, messageFunc, exception);
                }
                switch (logLevel) {
                    case LogLevel.Debug:
                        if (_logger.IsDebugEnabled) {
                            _logger.Debug(messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled) {
                            _logger.Info(messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled) {
                            _logger.Warn(messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled) {
                            _logger.Error(messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled) {
                            _logger.Fatal(messageFunc());
                            return true;
                        }
                        break;
                    default:
                        if (_logger.IsTraceEnabled) {
                            _logger.Trace(messageFunc());
                            return true;
                        }
                        break;
                }
                return false;
            }

            private bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
                switch (logLevel) {
                    case LogLevel.Debug:
                        if (_logger.IsDebugEnabled) {
                            _logger.DebugException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled) {
                            _logger.InfoException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled) {
                            _logger.WarnException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled) {
                            _logger.ErrorException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled) {
                            _logger.FatalException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    default:
                        if (_logger.IsTraceEnabled) {
                            _logger.TraceException(messageFunc(), exception);
                            return true;
                        }
                        break;
                }
                return false;
            }

            private bool IsLogLevelEnable(LogLevel logLevel) {
                switch (logLevel) {
                    case LogLevel.Debug:
                        return _logger.IsDebugEnabled;
                    case LogLevel.Info:
                        return _logger.IsInfoEnabled;
                    case LogLevel.Warn:
                        return _logger.IsWarnEnabled;
                    case LogLevel.Error:
                        return _logger.IsErrorEnabled;
                    case LogLevel.Fatal:
                        return _logger.IsFatalEnabled;
                    default:
                        return _logger.IsTraceEnabled;
                }
            }
        }
    }
}