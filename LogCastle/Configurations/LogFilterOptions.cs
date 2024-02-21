using System.Collections.Generic;

namespace LogCastle.Configurations
{
    public sealed class LogFilterOptions
    {
        public HashSet<string> IgnoreTypes { get; set; } = new HashSet<string>();
        public HashSet<string> IgnoreMethods { get; set; } = new HashSet<string>();  
    }
}
