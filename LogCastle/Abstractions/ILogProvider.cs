namespace LogCastle.Abstractions
{
    public interface ILogProvider
    {
        void Log(LogEntry logEntry);
    }
}
