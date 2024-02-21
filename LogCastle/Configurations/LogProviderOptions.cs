using System.Collections.Generic;

namespace LogCastle.Configurations
{
    public sealed class LogProviderOptions
    {
        public Dictionary<string, LogProviderConfiguration> Providers { get; set; } =
            new Dictionary<string, LogProviderConfiguration>();
    }

    public sealed class LogProviderConfiguration
    {
        public bool Enabled { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
