using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using System.Reflection;
using System.Linq;
using LogCastle.Abstractions;

namespace LogCastle.Factories
{
    public sealed class LogProxyControllersFactory : IControllerFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public LogProxyControllersFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public object CreateController(ControllerContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            var controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();
            var scope = _serviceScopeFactory.CreateScope();

            try
            {
                var parameterInfos = controllerType.GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault()
                    ?.GetParameters() ?? throw new InvalidOperationException("Uygun bir constructor bulunamadı.");

                var proxyFactory = scope.ServiceProvider.GetRequiredService<IProxyFactory>();
                var constructorArguments = GetParameters(parameterInfos, scope.ServiceProvider);
                var proxy = proxyFactory.CreateClassProxy(controllerType, constructorArguments);
                return proxy;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Controller oluşturulamadı: {context.ActionDescriptor.ControllerName}", ex);
            }           
        }

        public void ReleaseController(ControllerContext context, object controller)
        {
            switch (controller)
            {
                case IDisposable disposableController:
                    disposableController.Dispose();
                    break;
                // Eğer controller bir proxy ise ve hedef controller IDisposable implement ediyorsa
                case IProxyTargetAccessor proxyTargetAccessor:
                    {
                        var target = proxyTargetAccessor.DynProxyGetTarget();
                        if (target is IDisposable disposableTarget)
                        {
                            disposableTarget.Dispose();
                        }

                        break;
                    }
            }
        }

        private static object[] GetParameters(IReadOnlyList<ParameterInfo> parameterInfos, IServiceProvider provider)
        {
            var parameters = new object[parameterInfos.Count];
            for (var i = 0; i < parameterInfos.Count; i++)
            {
                parameters[i] = provider.GetRequiredService(parameterInfos[i].ParameterType);
            }
            return parameters;
        }
    }
}
