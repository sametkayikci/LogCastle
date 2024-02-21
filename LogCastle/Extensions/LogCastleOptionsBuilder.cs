using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LogCastle.Abstractions;
using LogCastle.Attributes;
using LogCastle.Configurations;
using LogCastle.Factories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LogCastle.Extensions
{
    public class LogCastleOptionsBuilder
    {
        private readonly IServiceCollection _services;
        private readonly LogCastleOptions _logCastleOptions;
        private bool _performAssemblyScan = true;
        private bool _decorateCalled = false;
            

        public LogCastleOptionsBuilder(IServiceCollection services, LogCastleOptions logCastleOptions)
        {
            _services = services;
            _logCastleOptions = logCastleOptions;
        }

        public LogCastleOptionsBuilder ConfigureOptions(Action<LogCastleOptions> configureOptions)
        {
            configureOptions?.Invoke(_logCastleOptions);
            return this;
        }

        public LogCastleOptionsBuilder ForControllers()
        {
            _services.AddTransient<IControllerFactory, LogProxyControllersFactory>();
            return this;
        }

        public LogCastleOptionsBuilder ForController<TController>()
            where TController : ControllerBase
        {
            _services.AddTransient<IControllerFactory, LogProxyControllerFactory<TController>>();
            return this;
        }

        public LogCastleOptionsBuilder ScanAssemblies(params Assembly[] assemblies)
        {
            if (_decorateCalled)
            {
                throw new InvalidOperationException("ScanAssemblies metodu, Decorate metodu çağrıldığında kullanılamaz.");
            }

            if (assemblies is null || assemblies.Length == 0)
            {
                throw new ArgumentException("En az bir assembly belirtilmelidir.", nameof(assemblies));
            }

            foreach (var assembly in assemblies)
            {
                var typesWithAttributes = assembly.GetTypes()
                    .Where(type => type.IsInterface)
                    .SelectMany(type => type.GetCustomAttributes<LogCastleAttribute>(inherit: true)
                        .Select(attr => new { ServiceType = type, Attribute = attr }))
                    .ToList();

                foreach (var typeWithAttribute in typesWithAttributes)
                {
                    var serviceDescriptor = _services.FirstOrDefault(descriptor =>
                        descriptor.ServiceType == typeWithAttribute.ServiceType);
                    if (serviceDescriptor != null)
                    {
                        _services.DecorateWithInterfaceProxy(typeWithAttribute.ServiceType,
                            serviceDescriptor.ImplementationType, serviceDescriptor.Lifetime);
                    }
                }
            }

            return this;
        }


        public LogCastleOptionsBuilder Decorate<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            _performAssemblyScan = false;

            // Decorate işlemleri burada devam ettirilir
            if (!typeof(TImplementation).GetInterfaces().Contains(typeof(TInterface)))
            {
                throw new InvalidOperationException(
                    $"{typeof(TImplementation).Name} türü, {typeof(TInterface).Name} arayüzünü uygulamıyor.");
            }

            var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(TInterface))
                                    ?? throw new InvalidOperationException(
                                        $"Servis türü {typeof(TInterface).Name} kayıtlı değil.");

            _services.Replace(ServiceDescriptor.Describe(
                serviceDescriptor.ServiceType,
                serviceProvider =>
                {
                    var proxyFactory = serviceProvider.GetRequiredService<IProxyFactory>();
                    var originalService = ((TImplementation)serviceProvider.GetService(typeof(TImplementation))
                                           ?? ActivatorUtilities.CreateInstance<TImplementation>(serviceProvider))
                                          ?? throw new InvalidOperationException(
                                              $"Orijinal servis {typeof(TImplementation).Name} oluşturulamadı.");

                    return proxyFactory.CreateInterfaceProxyWithTarget<TInterface>(originalService);
                },
                serviceDescriptor.Lifetime));

            // Decorate işlemi yapıldığında assembly taramasını devre dışı bırak
            _performAssemblyScan = false;
            _decorateCalled = true;

            return this;
        }

        public LogCastleOptionsBuilder For<TInterface, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    _services.AddSingleton(CreateProxy);
                    break;
                case ServiceLifetime.Scoped:
                    _services.AddScoped(CreateProxy);
                    break;
                case ServiceLifetime.Transient:
                    _services.AddTransient(CreateProxy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }

            return this;

            TInterface CreateProxy(IServiceProvider provider)
            {
                var proxyFactory = provider.GetRequiredService<IProxyFactory>();
                var implementation = ActivatorUtilities.CreateInstance<TImplementation>(provider);
                return proxyFactory.CreateInterfaceProxyWithTarget<TInterface>(implementation);
            }

        }

        public IServiceCollection Build()
        {
            return _services;
        }
    }
}
