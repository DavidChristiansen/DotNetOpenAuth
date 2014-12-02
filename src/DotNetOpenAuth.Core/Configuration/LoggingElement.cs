namespace DotNetOpenAuth.Configuration {
    using System.Configuration;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the &lt;logging&gt; element in the host's .config file.
    /// </summary>
    internal class LoggingElement : ConfigurationSection {
        /// <summary>
        /// The name of the @enabled attribute.
        /// </summary>
        private const string EnabledAttributeName = "enabled";

        /// <summary>
        /// The name of the &lt;logging&gt; sub-element.
        /// </summary>
        private const string LoggingElementName = DotNetOpenAuthSection.SectionName + "/logging";

        /// <summary>
        /// The name of the @loggerName attribute.
        /// </summary>
        private const string LoggerName = "logger";

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingElement"/> class.
        /// </summary>
        internal LoggingElement() {
        }

        /// <summary>
        /// Gets the configuration section from the .config file.
        /// </summary>
        public static LoggingElement Configuration {
            get {
                Contract.Ensures(Contract.Result<ReportingElement>() != null);
                return (LoggingElement)ConfigurationManager.GetSection(LoggingElementName) ?? new LoggingElement();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this reporting is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [ConfigurationProperty(EnabledAttributeName, DefaultValue = true)]
        internal bool Enabled {
            get { return (bool)this[EnabledAttributeName]; }
            set { this[EnabledAttributeName] = value; }
        }

        /// <summary>
        /// Gets or sets the logger to use.
        /// </summary>
        [ConfigurationProperty(LoggerName, DefaultValue = "NoOp")] // 1 day default
        internal string Logger {
            get { return (string)this[LoggerName]; }
            set { this[LoggerName] = value; }
        }
    }
}