using LogCastle.Abstractions;

namespace LogCastle.Formatters
{
    public sealed class CustomLogFormatter : ILogFormatter
    {
        public string Format(LogEntry logEntry)
        {
            return $"{logEntry.Message}";
        }
    }
}
