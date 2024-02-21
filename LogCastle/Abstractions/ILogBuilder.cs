using System;
using LogCastle.Enums;

namespace LogCastle.Abstractions
{
    public interface ILogBuilder
    {
        ILogBuilder AppendNamespace(string fullMethodName);
        ILogBuilder AppendArguments(string arguments);
        ILogBuilder AppendApplicationName(string applicationName);
        ILogBuilder AppendHost(string hostname);
        ILogBuilder AppendElapsedTime(long milliseconds);
        ILogBuilder AppendReturnValue(string returnValue);
        ILogBuilder AppendError(string error);
        ILogBuilder AppendLevel(Level level);
        ILogBuilder AppendTimeStamp(DateTime timeStamp);
        ILogBuilder AppendMessage(string message);
        LogEntry Build();
    }
}