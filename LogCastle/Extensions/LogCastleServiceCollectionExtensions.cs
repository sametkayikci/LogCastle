using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using LogCastle.Abstractions;
using LogCastle.Caching;
using LogCastle.Configurations;
using LogCastle.Factories;
using LogCastle.Formatters;
using LogCastle.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogCastle.Extensions
{
    public static class LogCastleServiceCollectionExtensions
    {
        public static LogCastleOptionsBuilder AddLogCastles(this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var logCastleOptions = new LogCastleOptions();
            services.Configure<LogCastleOptions>(configuration.GetSection("LogCastle"));
            services.AddSingleton(logCastleOptions);

            services.AddSingleton<ILogFormatter, CustomLogFormatter>();
            services.AddSingleton<IControllerBaseMethodCache, ControllerBaseMethodCache>();
            services.AddSingleton<IInterceptor, LogInterceptor>();
            services.AddTransient<IProxyGenerator, ProxyGenerator>();
            services.AddTransient<IProxyFactory, ProxyFactory>();

            var providerOptions = new LogProviderOptions();
            configuration.GetSection("LogCastle:Providers").Bind(providerOptions.Providers);

            foreach (var providerConfig in providerOptions.Providers)
            {
                if (!providerConfig.Value.Enabled) continue;

                var type = Type.GetType(providerConfig.Value.Type);
                if (type is null || !typeof(ILogProvider).IsAssignableFrom(type)) continue;

                if (providerConfig.Value.Parameters != null && providerConfig.Value.Parameters.Any())
                {
                    services.AddSingleton(typeof(ILogProvider), serviceProvider =>
                    {
                        var constructor = type.GetConstructor(new[] { typeof(IReadOnlyDictionary<string, string>) });
                        return constructor != null
                            ? (ILogProvider)constructor.Invoke(new object[] { providerConfig.Value.Parameters })
                            : throw new InvalidOperationException(
                                "The constructor for the specified type could not be found.");
                    });
                }
                else
                {
                    services.AddSingleton(typeof(ILogProvider),
                        serviceProvider => (ILogProvider)Activator.CreateInstance(type));
                }
            }

            return new LogCastleOptionsBuilder(services, logCastleOptions);
        }
    }
}