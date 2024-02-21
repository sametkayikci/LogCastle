using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using LogCastle.Abstractions;
using LogCastle.Enums;
using LogCastle.Extensions;
using LogCastle.Providers;

namespace LogCastle.Logging
{
    public static class Logger
    {
        private static ILogProvider LogProvider { get; } = new ConsoleLogProvider();

        public static void LogTrace(string message)
        {
            Log(Level.Trace, message);
        }

        public static void LogDebug(string message)
        {
            Log(Level.Debug, message);
        }

        public static void LogInformation(string message)
        {
            Log(Level.Information, message);
        }

        public static void LogWarning(string message)
        {
            Log(Level.Warning, message);
        }

        public static void LogError(string message, Exception exception)
        {
            Log(Level.Error, $"{message} - {exception.ToFullErrorMessage()}");
        }

        public static void LogFatal(string message)
        {
            Log(Level.Fatal, message);
        }
        public static void LogCritical(string message)
        {
            Log(Level.Critical, message);
        }

        public static void NamespaceName([CallerMemberName] string namespaceName = null)
        {
            Log(Level.Critical, namespaceName);
          
        }

        private static void Log(Level level, string message)
        {
            var logBuilder = new LogBuilder();
            logBuilder.AppendTimeStamp(DateTime.UtcNow)
                .AppendLevel(level)
                .AppendApplicationName(Assembly.GetEntryAssembly()?.GetName().Name)
                .AppendHost(Environment.MachineName)
                .AppendMessage(message);

            var logEntry = logBuilder.Build();

            LogProvider.Log(logEntry);
        }      
    }
}
