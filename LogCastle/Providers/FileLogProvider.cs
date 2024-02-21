using LogCastle.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogCastle.Providers
{
    public sealed class FileLogProvider : ILogProvider
    {
        private readonly string _filePath;

        public FileLogProvider(IReadOnlyDictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("FilePath", out _filePath))
            {
                throw new ArgumentException("FilePath parametresi zorunludur.");
            }
        }

        public void Log(LogEntry logEntry)
        {
            File.AppendAllText(_filePath, logEntry.Message + Environment.NewLine);
        }
    }
}
