using LogCastle.Enums;
using System;
using static System.AttributeTargets;

namespace LogCastle.Attributes
{
    [AttributeUsage(Class | Method | Interface, Inherited = false)]
    public sealed class LogCastleAttribute : Attribute
    {
        public Level Level { get; }

        public LogCastleAttribute(Level level = Level.Information)
        {
            Level = level;
        }
    }
}