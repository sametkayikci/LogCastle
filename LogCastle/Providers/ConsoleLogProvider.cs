using LogCastle.Abstractions;
using System;

namespace LogCastle.Providers
{
    public sealed class ConsoleLogProvider : ILogProvider
    {
        public void Log(LogEntry logEntry)
        {
            Console.WriteLine(logEntry.Message);
        }
    }
}
