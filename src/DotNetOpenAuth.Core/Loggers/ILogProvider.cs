namespace DotNetOpenAuth.Loggers {
    /// <summary>
    /// Represents a way to get a <see cref="ILog"/>
    /// </summary>
    public interface ILogProvider {
        ILog GetLogger(string name);
    }
}