using Castle.DynamicProxy;
using LogCastle.Abstractions;
using LogCastle.Configurations;
using LogCastle.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using LogCastle.Attributes;
using LogCastle.Logging;

namespace LogCastle.Interceptors
{
    public sealed class LogInterceptor : IInterceptor
    {
        private readonly LogCastleOptions _logOptions;
        private readonly ILogFormatter _logFormatter;
        private readonly IEnumerable<ILogProvider> _logProviders;
        private readonly IControllerBaseMethodCache _controllerBaseMethodCache;

        public LogInterceptor(IOptions<LogCastleOptions> logOptions,
            ILogFormatter logFormatter,
            IEnumerable<ILogProvider> logProviders,
            IControllerBaseMethodCache controllerBaseMethodCache)
        {
            _logOptions = logOptions?.Value ??
                          throw new ArgumentNullException(nameof(logOptions));
            _logFormatter = logFormatter ??
                            throw new ArgumentNullException(nameof(logFormatter));
            _logProviders = logProviders ??
                            throw new ArgumentNullException(nameof(logProviders));
            _controllerBaseMethodCache = controllerBaseMethodCache ??
                                         throw new ArgumentNullException(nameof(controllerBaseMethodCache));
        }

        /// <summary>
        /// Metod çağrılarını yakalayarak LogCastleAttribute ile tanımlanmış loglama işlemlerini gerçekleştirir.
        /// Loglama işlemi sırasıyla metot, sınıf ve arayüz düzeyindeki LogCastleAttribute özelliklerini kontrol eder.
        /// Eğer bir hata oluşursa, hata bilgisi de loglanır.
        /// </summary>
        /// <param name="invocation">Çağrılan metodu ve onun bilgilerini içeren IInvocation nesnesi.</param>
        /// <remarks>
        /// Eğer sınıf, interface veya metot düzeyinde bir LogCastleAttribute tanımlanmamışsa, loglama işlemi yapılmaz.
        /// Loglama işlemi, LogCastleAttribute ile belirlenen log seviyesine ve genel loglama yapılandırmalarına
        /// bağlı olarak gerçekleştirilir.
        /// </remarks>
        public void Intercept(IInvocation invocation)
        {
            var logAttribute = GetLogAttribute(invocation);
            if (logAttribute is null || !ShouldProceedLogging(invocation, logAttribute))
            {
                invocation.Proceed();
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var logBuilder = new LogBuilder();
            logBuilder.AppendTimeStamp(DateTime.UtcNow)
                .AppendLevel(logAttribute.Level)
                .AppendNamespace(invocation.GetFullMethodName())
                .AppendArguments(invocation.GetMaskedArguments())
                .AppendHost(Environment.MachineName)
                .AppendApplicationName(invocation.TargetType.Module.Name);

            try
            {
                invocation.Proceed();
                stopwatch.Stop();
                logBuilder.AppendElapsedTime(stopwatch.ElapsedMilliseconds)
                    .AppendReturnValue(invocation.ReturnValue?.ToDetailedLogString());
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logBuilder.AppendElapsedTime(stopwatch.ElapsedMilliseconds)
                    .AppendError(ex.ToFullErrorMessage());
                throw;
            }
            finally
            {
                var logEntry = logBuilder.Build();
                var formattedLogMessage = _logFormatter.Format(logEntry);
                LogToProviders(formattedLogMessage);
            }
        }


        private void LogToProviders(string formattedLogMessage)
        {
            foreach (var provider in _logProviders)
            {
                provider.Log(new LogEntry
                {
                    Message = formattedLogMessage,
                });
            }
        }

        private static LogCastleAttribute GetLogAttribute(IInvocation invocation)
        {
            return invocation.GetLogCastleAttribute();
        }

        private bool ShouldProceedLogging(IInvocation invocation, LogCastleAttribute logAttribute)
        {
            if (!_logOptions.Enabled) return false;
            if (ShouldIgnoreLogging(invocation)) return false;
            if (logAttribute.Level < _logOptions.MinimumLevel) return false;

            return true;
        }

        private bool ShouldIgnoreLogging(IInvocation invocation)
        {
            if (_controllerBaseMethodCache.IsMethodFromControllerBase(invocation.Method))
                return true;

            return _logOptions.Filter.IgnoreTypes.Contains(invocation.TargetType.FullName) ||
                   _logOptions.Filter.IgnoreMethods.Contains(invocation.Method.Name);
        }
    }
}