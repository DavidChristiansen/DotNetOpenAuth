namespace DotNetOpenAuth.Loggers {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public class ColouredConsoleLogProvider : ILogProvider {
        static ColouredConsoleLogProvider() {
            MessageFormatter = DefaultMessageFormatter;
            Colors = new Dictionary<LogLevel, ConsoleColor> {
                { LogLevel.Fatal, ConsoleColor.Red },
                { LogLevel.Error, ConsoleColor.Yellow },
                { LogLevel.Warn, ConsoleColor.Magenta },
                { LogLevel.Info, ConsoleColor.White },
                { LogLevel.Debug, ConsoleColor.Gray },
                { LogLevel.Trace, ConsoleColor.DarkGray },
            };
        }

        public ILog GetLogger(string name) {
            return new ColouredConsoleLogger(name);
        }

        /// <summary>
        /// A delegate returning a formatted log message
        /// </summary>
        /// <param name="loggerName">The name of the Logger</param>
        /// <param name="level">The Log Level</param>
        /// <param name="message">The Log Message</param>
        /// <param name="e">The Exception, if there is one</param>
        /// <returns>A formatted Log Message string.</returns>
        public delegate string MessageFormatterDelegate(
            string loggerName,
            LogLevel level,
            object message,
            Exception e);

        public static Dictionary<LogLevel, ConsoleColor> Colors { get; set; }

        public static MessageFormatterDelegate MessageFormatter { get; set; }

        protected static string DefaultMessageFormatter(string loggerName, LogLevel level, object message, Exception e) {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture));

            stringBuilder.Append(" ");

            // Append a readable representation of the log level
            stringBuilder.Append(("[" + level.ToString().ToUpper() + "]").PadRight(8));

            stringBuilder.Append("(" + loggerName + ") ");

            // Append the message
            stringBuilder.Append(message);

            // Append stack trace if there is an exception
            if (e != null) {
                stringBuilder.Append(Environment.NewLine).Append(e.GetType());
                stringBuilder.Append(Environment.NewLine).Append(e.Message);
                stringBuilder.Append(Environment.NewLine).Append(e.StackTrace);
            }

            return stringBuilder.ToString();
        }

        public class ColouredConsoleLogger : ILog {
            private readonly string _name;

            public ColouredConsoleLogger(string name) {
                _name = name;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
                if (messageFunc == null) {
                    return true;
                }

                Write(logLevel, messageFunc(), exception);
                return true;
            }

            protected void Write(LogLevel logLevel, string message, Exception e = null) {
                var formattedMessage = MessageFormatter(this._name, logLevel, message, e);
                ConsoleColor color;

                if (Colors.TryGetValue(logLevel, out color)) {
                    var originalColor = Console.ForegroundColor;
                    try {
                        Console.ForegroundColor = color;
                        Console.Out.WriteLine(formattedMessage);
                    } finally {
                        Console.ForegroundColor = originalColor;
                    }
                } else {
                    Console.Out.WriteLine(formattedMessage);
                }
            }
        }
    }
}