using LogCastle.Logging;
using System;

namespace LogCastle.Extensions
{
    public static class ExceptionExtensions
    {
        public static string ToFullErrorMessage(this Exception exception)
        {
            if (exception is null)
                return string.Empty;

            var logBuilder = new LogBuilder();
            logBuilder.AppendError($"{exception.Message},[StackTrace] {exception.StackTrace}");

            var innerException = exception.InnerException;
            var innerCount = 1;
            while (innerException != null)
            {
                logBuilder.AppendMessage(
                    $"| [InnerException] {innerCount}: [Error] {innerException.Message}, [StackTrace] {innerException.StackTrace}");
                innerException = innerException.InnerException;
                innerCount++;
            }

            return logBuilder.Build().Message;
        }
    }
}
