namespace LogCastle.Abstractions
{
    public interface ILogFormatter
    {
        string Format(LogEntry logEntry);
    }
}
