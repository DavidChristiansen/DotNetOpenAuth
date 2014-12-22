namespace DotNetOpenAuth.Loggers {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public class SerilogLogProvider : ILogProvider {
        private readonly Func<string, object> _getLoggerByNameDelegate;
        private static bool _providerIsAvailableOverride = true;

        public SerilogLogProvider() {
            if (!IsLoggerAvailable()) {
                throw new InvalidOperationException("Serilog.Log not found");
            }
            _getLoggerByNameDelegate = GetForContextMethodCall();
        }

        public static bool ProviderIsAvailableOverride {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name) {
            return new SerilogLogger(_getLoggerByNameDelegate(name));
        }

        public static bool IsLoggerAvailable() {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetLogManagerType() {
            return Type.GetType("Serilog.Log, Serilog");
        }

        private static Func<string, object> GetForContextMethodCall() {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethod("ForContext", new[] { typeof(string), typeof(object), typeof(bool) });
            ParameterExpression propertyNameParam = Expression.Parameter(typeof(string), "propertyName");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
            ParameterExpression destructureObjectsParam = Expression.Parameter(typeof(bool), "destructureObjects");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[]
            {
                propertyNameParam, 
                valueParam,
                destructureObjectsParam
            });
            var func = Expression.Lambda<Func<string, object, bool, object>>(methodCall, new[]
            {
                propertyNameParam,
                valueParam,
                destructureObjectsParam
            }).Compile();
            return name => func("Name", name, false);
        }

        public class SerilogLogger : ILog {
            private readonly object _logger;
            private static readonly object DebugLevel;
            private static readonly object ErrorLevel;
            private static readonly object FatalLevel;
            private static readonly object InformationLevel;
            private static readonly object VerboseLevel;
            private static readonly object WarningLevel;
            private static readonly Func<object, object, bool> IsEnabled;
            private static readonly Action<object, object, string> Write;
            private static readonly Action<object, object, Exception, string> WriteException;

            static SerilogLogger() {
                var logEventTypeType = Type.GetType("Serilog.Events.LogEventLevel, Serilog");
                DebugLevel = Enum.Parse(logEventTypeType, "Debug");
                ErrorLevel = Enum.Parse(logEventTypeType, "Error");
                FatalLevel = Enum.Parse(logEventTypeType, "Fatal");
                InformationLevel = Enum.Parse(logEventTypeType, "Information");
                VerboseLevel = Enum.Parse(logEventTypeType, "Verbose");
                WarningLevel = Enum.Parse(logEventTypeType, "Warning");

                // Func<object, object, bool> isEnabled = (logger, level) => { return ((SeriLog.ILogger)logger).IsEnabled(level); }
                var loggerType = Type.GetType("Serilog.ILogger, Serilog");
                MethodInfo isEnabledMethodInfo = loggerType.GetMethod("IsEnabled");
                ParameterExpression instanceParam = Expression.Parameter(typeof(object));
                UnaryExpression instanceCast = Expression.Convert(instanceParam, loggerType);
                ParameterExpression levelParam = Expression.Parameter(typeof(object));
                UnaryExpression levelCast = Expression.Convert(levelParam, logEventTypeType);
                MethodCallExpression isEnabledMethodCall = Expression.Call(instanceCast, isEnabledMethodInfo, levelCast);
                IsEnabled = Expression.Lambda<Func<object, object, bool>>(isEnabledMethodCall, new[]
                {
                    instanceParam,
                    levelParam
                }).Compile();

                // Action<object, object, string> Write =
                // (logger, level, message) => { ((SeriLog.ILoggerILogger)logger).Write(level, message, new object[]); }
                MethodInfo writeMethodInfo = loggerType.GetMethod("Write", new[] { logEventTypeType, typeof(string), typeof(object[]) });
                ParameterExpression messageParam = Expression.Parameter(typeof(string));
                ConstantExpression propertyValuesParam = Expression.Constant(new object[0]);
                MethodCallExpression writeMethodExp = Expression.Call(instanceCast, writeMethodInfo, levelCast, messageParam, propertyValuesParam);
                Write = Expression.Lambda<Action<object, object, string>>(writeMethodExp, new[]
                {
                    instanceParam,
                    levelParam,
                    messageParam
                }).Compile();

                // Action<object, object, string, Exception> WriteException =
                // (logger, level, exception, message) => { ((ILogger)logger).Write(level, exception, message, new object[]); }
                MethodInfo writeExceptionMethodInfo = loggerType.GetMethod("Write", new[]
                {
                    logEventTypeType,
                    typeof(Exception), 
                    typeof(string),
                    typeof(object[])
                });
                ParameterExpression exceptionParam = Expression.Parameter(typeof(Exception));
                writeMethodExp = Expression.Call(
                    instanceCast,
                    writeExceptionMethodInfo,
                    levelCast,
                    exceptionParam,
                    messageParam,
                    propertyValuesParam);
                WriteException = Expression.Lambda<Action<object, object, Exception, string>>(writeMethodExp, new[]
                {
                    instanceParam,
                    levelParam,
                    exceptionParam,
                    messageParam,
                }).Compile();
            }

            internal SerilogLogger(object logger) {
                _logger = logger;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
                if (messageFunc == null) {
                    return IsEnabled(_logger, logLevel);
                }
                if (exception != null) {
                    return LogException(logLevel, messageFunc, exception);
                }

                switch (logLevel) {
                    case LogLevel.Debug:
                        if (IsEnabled(_logger, DebugLevel)) {
                            Write(_logger, DebugLevel, messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Info:
                        if (IsEnabled(_logger, InformationLevel)) {
                            Write(_logger, InformationLevel, messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Warn:
                        if (IsEnabled(_logger, WarningLevel)) {
                            Write(_logger, WarningLevel, messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Error:
                        if (IsEnabled(_logger, ErrorLevel)) {
                            Write(_logger, ErrorLevel, messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Fatal:
                        if (IsEnabled(_logger, FatalLevel)) {
                            Write(_logger, FatalLevel, messageFunc());
                            return true;
                        }
                        break;
                    default:
                        if (IsEnabled(_logger, VerboseLevel)) {
                            Write(_logger, VerboseLevel, messageFunc());
                            return true;
                        }
                        break;
                }
                return false;
            }

            private bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
                switch (logLevel) {
                    case LogLevel.Debug:
                        if (IsEnabled(_logger, DebugLevel)) {
                            WriteException(_logger, DebugLevel, exception, messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Info:
                        if (IsEnabled(_logger, InformationLevel)) {
                            WriteException(_logger, InformationLevel, exception, messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Warn:
                        if (IsEnabled(_logger, WarningLevel)) {
                            WriteException(_logger, WarningLevel, exception, messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Error:
                        if (IsEnabled(_logger, ErrorLevel)) {
                            WriteException(_logger, ErrorLevel, exception, messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Fatal:
                        if (IsEnabled(_logger, FatalLevel)) {
                            WriteException(_logger, FatalLevel, exception, messageFunc());
                            return true;
                        }
                        break;
                    default:
                        if (IsEnabled(_logger, VerboseLevel)) {
                            WriteException(_logger, VerboseLevel, exception, messageFunc());
                            return true;
                        }
                        break;
                }
                return false;
            }
        }
    }
}