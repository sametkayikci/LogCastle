using Castle.DynamicProxy;
using LogCastle.Abstractions;
using LogCastle.Attributes;
using LogCastle.Caching;
using LogCastle.Configurations;
using LogCastle.Factories;
using LogCastle.Formatters;
using LogCastle.Interceptors;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace LogCastle.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// LogCastle kütüphanesini yapılandırmak için gerekli servisleri ve log sağlayıcılarını ekler.
        /// </summary>
        /// <param name="services">Servis koleksiyonu.</param>
        /// <param name="configuration">Uygulamanın yapılandırma bilgileri.</param>
        /// <returns>Yapılandırılmış servis koleksiyonu.</returns>
        /// <remarks>
        /// Bu metod, LogCastle kütüphanesinin log sağlayıcılarını ve gerekli olan diğer servisleri ekler.
        /// Yapılandırma bilgisi "LogCastle" bölümünden alınır. Log sağlayıcıları "LogCastle:Providers" altında tanımlanır.
        /// Her bir log sağlayıcısı için "Enabled" özelliği kontrol edilir ve eğer true ise sağlayıcı servis koleksiyonuna eklenir.
        /// Log sağlayıcılarının tip bilgisi "Type" özelliğinden alınır ve dinamik olarak yaratılır.
        /// Eğer log sağlayıcısının bir constructor parametresi varsa ve "Parameters" altında yapılandırılmışsa, bu parametreler
        /// kullanılarak sağlayıcı örneği oluşturulur.
        /// </remarks>
        public static IServiceCollection AddLogCastleConfigurations(this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));
            
            services.Configure<LogCastleOptions>(configuration.GetSection("LogCastle"));
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
                        if (constructor != null)
                        {
                            return (ILogProvider)constructor.Invoke(new object[] { providerConfig.Value.Parameters });
                        }

                        throw new InvalidOperationException(
                            "The constructor for the specified type could not be found.");
                    });
                }
                else
                {
                    services.AddSingleton(typeof(ILogProvider),
                        serviceProvider => (ILogProvider)Activator.CreateInstance(type));
                }
            }

            return services;
        }

        /// <summary>
        /// Belirtilen TInterface ve TImplementation türlerindeki interface ve sınıf için LogCastle proxy'sini oluşturur ve belirtilen yaşam döngüsüne göre servis koleksiyonuna ekler.
        /// Bu metot, hem interface hem de onun gerçekleştirmesi olan sınıf bazında proxy oluşturma ve kayıt işlemi içindir.
        /// </summary>
        /// <typeparam name="TInterface">Proxy'si oluşturulacak olan interface.</typeparam>
        /// <typeparam name="TImplementation">Proxy'si oluşturulacak olan sınıf. Bu sınıfın, belirtilen interface'i uygulamalıdır ve bir parametresiz yapıcıya sahip olmalıdır.</typeparam>
        /// <param name="services">IServiceCollection örneği.</param>
        /// <param name="lifetime">Servisin yaşam döngüsü. Varsayılan değeri Scoped'tır.</param>
        /// <returns>IServiceCollection örneğini döndürür, bu da diğer konfigürasyonlar için method chaining sağlar.</returns>
        /// <example>
        /// Bu metodu kullanarak interface ve sınıf bazlı proxy'li bir servisi kaydedebilirsiniz:
        /// <code>
        /// services.AddLogCastle&lt;IMyInterface, MyImplementationWithInterface&gt;(ServiceLifetime.Singleton);
        /// </code>
        /// Veya varsayılan yaşam döngüsünü (Scoped) kullanarak:
        /// <code>
        /// services.AddLogCastle&lt;IMyInterface, MyImplementationWithInterface&gt;();
        /// </code>
        /// </example>
        public static IServiceCollection AddLogCastle<TInterface, TImplementation>(this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(CreateProxy);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(CreateProxy);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(CreateProxy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }

            return services;

            TInterface CreateProxy(IServiceProvider provider)
            {
                var proxyFactory = provider.GetRequiredService<IProxyFactory>();
                var implementation = ActivatorUtilities.CreateInstance<TImplementation>(provider);
                return proxyFactory.CreateInterfaceProxyWithTarget<TInterface>(implementation);
            }
        }


        /// <summary>
        /// Belirtilen TImplementation türündeki sınıf için <see cref="LogCastleAttribute"/> ile işaretlenmiş proxy'sini oluşturur ve belirtilen yaşam döngüsüne göre servis koleksiyonuna ekler.
        /// Bu metot, sınıf bazlı proxy oluşturma ve kayıt işlemi içindir.
        /// </summary>
        /// <remarks><see cref="LogCastleAttribute"/> ile işaretlenmiş metotlar mutlaka virtual ile işaretlenmelidir!</remarks>
        /// <typeparam name="TImplementation">Proxy'si oluşturulacak olan sınıf. Bu sınıfın bir parametresiz yapıcıya sahip olması gerekir.</typeparam>
        /// <param name="services">IServiceCollection örneği.</param>
        /// <param name="lifetime">Servisin yaşam döngüsü. Varsayılan değeri Scoped'tır.</param>
        /// <returns>IServiceCollection örneğini döndürür, bu da diğer konfigürasyonlar için method chaining sağlar.</returns>
        /// <example>
        /// Bu metodu kullanarak sınıf bazlı proxy'li bir servisi kaydedebilirsiniz:
        /// <code>
        /// services.AddLogCastle&lt;MyService&gt;(ServiceLifetime.Singleton);
        /// </code>
        /// Veya varsayılan yaşam döngüsünü (Scoped) kullanarak:
        /// <code>
        /// services.AddLogCastle&lt;MyService&gt;();
        /// </code>
        /// </example>
        public static IServiceCollection AddLogCastle<TImplementation>(this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TImplementation : class
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(CreateProxy);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(CreateProxy);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(CreateProxy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }

            return services;

            TImplementation CreateProxy(IServiceProvider provider)
            {
                var proxyFactory = provider.GetRequiredService<IProxyFactory>();
                var implementation = ActivatorUtilities.CreateInstance<TImplementation>(provider);
                return proxyFactory.CreateClassProxyWithTarget(implementation);
            }
        }


        /// <summary>
        /// Belirtilen TInterface tipindeki servisleri, TImplementation tipindeki implementasyonları kullanarak LogCastle ile dekore eder.
        /// Bu işlem, loglama ve diğer çapraz kesitsel kaygıları uygulamak için kullanılır.
        /// </summary>
        /// <typeparam name="TInterface">Dekore edilecek servisin arayüz tipi.</typeparam>
        /// <typeparam name="TImplementation">Arayüzü uygulayan ve dekore edilecek sınıfın tipi.</typeparam>
        /// <param name="services">IServiceCollection nesnesi.</param>
        /// <returns>IServiceCollection örneğini döndürür, bu da diğer konfigürasyonlar için method chaining sağlar.</returns>
        /// <exception cref="InvalidOperationException">
        /// Eğer TInterface tipinde bir servis kayıtlı değilse veya TImplementation tipinde bir nesne oluşturulamazsa fırlatılır.
        /// </exception>
        public static IServiceCollection DecorateWithLogCastle<TInterface, TImplementation>(
            this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            if (!typeof(TImplementation).GetInterfaces().Contains(typeof(TInterface)))
            {
                throw new InvalidOperationException(
                    $"{typeof(TImplementation).Name} türü, {typeof(TInterface).Name} arayüzünü uygulamıyor.");
            }

            var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TInterface))
                                    ?? throw new InvalidOperationException(
                                        $"Servis türü {typeof(TInterface).Name} kayıtlı değil.");

            services.Replace(ServiceDescriptor.Describe(
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

            return services;
        }


        /// <summary>
        /// Belirli bir controller için LogCastle ekler.
        /// </summary>
        /// <typeparam name="TController">Loglaması yapılacak controller sınıfı.</typeparam>
        /// <param name="services">IServiceCollection nesnesi, genellikle Startup sınıfında yapılandırma sırasında kullanılır.</param>
        /// <returns>Yapılandırılmış IServiceCollection nesnesi ile birlikte zincirleme yapılandırmaya olanak tanır.</returns>
        public static IServiceCollection AddLogCastleForController<TController>(this IServiceCollection services)
            where TController : ControllerBase
        {
            services.AddTransient<IControllerFactory, LogProxyControllerFactory<TController>>();
            return services;
        }

        /// <summary>
        /// Uygulamadaki tüm controllerlar için LogCastle ekler.
        /// </summary>
        /// <param name="services">Servis koleksiyonu.</param>
        /// <returns>IServiceCollection örneğini döndürür, bu da diğer konfigürasyonlar için method chaining sağlar.</returns>
        public static IServiceCollection AddLogCastleForControllers(this IServiceCollection services)
        {
            services.AddTransient<IControllerFactory, LogProxyControllersFactory>();
            return services;
        }

        internal static IServiceCollection DecorateWithInterfaceProxy(this IServiceCollection services,
            Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == serviceType)
                                    ?? throw new InvalidOperationException(
                                        $"Servis türü {serviceType.Name} kayıtlı değil.");

            services.Remove(serviceDescriptor);
            services.Add(ServiceDescriptor.Describe(
                serviceType,
                serviceProvider =>
                {
                    var proxyFactory = serviceProvider.GetRequiredService<IProxyFactory>();
                    var originalService = serviceProvider.GetService(implementationType)
                                          ?? ActivatorUtilities.CreateInstance(serviceProvider, implementationType);

                    return proxyFactory.CreateInterfaceProxyWithTarget(serviceType, originalService);
                },
                lifetime));

            return services;
        }

        /// <summary>
        /// Belirtilen assembly'de, LogCastleAttribute ile işaretlenmiş interfaceleri 
        /// ve bunların implementasyonlarını bulup, bunlar için proxy yaratır.
        /// </summary>
        /// <param name="services">Servis koleksiyonu.</param>
        /// <param name="assemblies">Loglama işlevselliğinin uygulanacağı interfacelerin bulunduğu assembly listesi</param>
        /// <returns>IServiceCollection örneğini döndürür, bu da diğer konfigürasyonlar için method chaining sağlar.</returns>
        /// <remarks>
        /// Bu metod, LogCastleAttribute ile işaretlenmiş her interface için,
        /// mevcut servis kaydını proxy ile dekore eder. Her dekore edilmiş servis, özgün servis kaydının
        /// yaşam süresini (scoped, transient, singleton) korur.
        /// </remarks>
        public static IServiceCollection AddLogCastle(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var typesWithAttributes = assembly.GetTypes()
                    .Where(type => type.IsInterface)
                    .SelectMany(type => type.GetCustomAttributes<LogCastleAttribute>(inherit: true)
                        .Select(attr => new { ServiceType = type, Attribute = attr }))
                    .ToList();

                foreach (var typeWithAttribute in typesWithAttributes)
                {
                    var serviceDescriptor = services.SingleOrDefault(descriptor =>
                        descriptor.ServiceType == typeWithAttribute.ServiceType);
                    if (serviceDescriptor != null)
                    {
                        services.DecorateWithInterfaceProxy(typeWithAttribute.ServiceType,
                            serviceDescriptor.ImplementationType,
                            serviceDescriptor.Lifetime);
                    }
                }
            }

            return services;
        }     
    }   
   
}