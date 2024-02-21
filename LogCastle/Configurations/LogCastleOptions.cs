using LogCastle.Enums;

namespace LogCastle.Configurations
{
    public sealed class LogCastleOptions
    {
        public bool Enabled { get; set; } = true;
        public Level MinimumLevel { get; set; } = Level.Information;
        public LogFilterOptions Filter { get; set; } = new LogFilterOptions();
    }
}
