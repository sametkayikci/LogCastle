using System;
using LogCastle.Abstractions;

namespace LogCastle.Logging
{
    public sealed class ValueTypeLoggable : ILoggable
    {
        private readonly ValueType _value;
        public ValueTypeLoggable(ValueType value)
        {
            _value = value;
        }
        public string ToLogString() => _value.ToString();
    }
}