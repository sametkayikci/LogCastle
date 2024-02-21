using LogCastle.Abstractions;

namespace LogCastle.Logging
{
    public sealed class NullLoggable : ILoggable
    {
        public string ToLogString() => "null";
    }
}