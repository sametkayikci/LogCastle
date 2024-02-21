using System;
using System.Text;
using LogCastle.Abstractions;
using LogCastle.Enums;

namespace LogCastle.Logging
{
    public class LogBuilder : ILogBuilder
    {
        private readonly StringBuilder _logMessage = new StringBuilder();
        public ILogBuilder AppendNamespace(string fullMethodName)
        {
            _logMessage.Append($"[Namespace] {fullMethodName} ");
            return this;
        }

        public ILogBuilder AppendArguments(string arguments)
        {
            _logMessage.Append($"[Args] {arguments} ");
            return this;
        }

        public ILogBuilder AppendHost(string hostname)
        {
            _logMessage.Append($"[Host] {hostname} ");
            return this;
        }

        public ILogBuilder AppendApplicationName(string applicationName)
        {
            _logMessage.Append($"[AppName] {applicationName} ");
            return this;
        }

        public ILogBuilder AppendElapsedTime(long milliseconds)
        {
            _logMessage.Append($"[ElapsedTime] {milliseconds} ms ");
            return this;
        }

        public ILogBuilder AppendReturnValue(string returnValue)
        {
            _logMessage.Append($"[ReturnValue] {returnValue} ");
            return this;
        }

        public ILogBuilder AppendError(string error)
        {
            _logMessage.Append($"[Error] {error} ");
            return this;
        }

        public ILogBuilder AppendLevel(Level level)
        {
            _logMessage.Append($"[Level] {level} ");
            return this;
        }

        public ILogBuilder AppendTimeStamp(DateTime timeStamp)
        {
            _logMessage.Append($"[TimeStamp] {timeStamp} ");
            return this;
        }

        public ILogBuilder AppendMessage(string message)
        {
            _logMessage.Append(message);
            return this;
        }

        public LogEntry Build()
        {
            return new LogEntry
            {
                Message = _logMessage.ToString(),
            };
        }
    }
}