using System.Collections;
using LogCastle.Abstractions;
using LogCastle.Extensions;

namespace LogCastle.Logging
{
    public sealed class EnumerableLoggable : ILoggable
    {
        private readonly IEnumerable _enumerable;

        public EnumerableLoggable(IEnumerable enumerable)
        {
            _enumerable = enumerable;
        }

        public string ToLogString()
        {
            return _enumerable.ToEnumerableString();
        }
    }
}