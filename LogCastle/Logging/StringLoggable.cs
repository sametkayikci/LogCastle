using LogCastle.Abstractions;
using LogCastle.Extensions;

namespace LogCastle.Logging
{
    public sealed class StringLoggable : ILoggable
    {
        private readonly string _value;
        public StringLoggable(string value)
        {
            _value = value;
        }

        public string ToLogString()
        {            
            return _value.ToMaskString();
        }
    }
}