using System.Linq;
using LogCastle.Abstractions;
using LogCastle.Extensions;

namespace LogCastle.Logging
{
    public sealed class DefaultLoggable : ILoggable
    {
        private readonly object _object;

        public DefaultLoggable(object obj)
        {
            _object = obj;
        }

        public string ToLogString()
        {
            if (_object is null) return "null";

            var properties = _object.GetType().GetProperties()
                .Where(prop => prop.CanRead)
                .Select(prop => prop.ToMaskedOrSerializedPropertyString(_object));

            return $"{{{string.Join(", ", properties)}}}";
        }
    }
}