using System;
using Castle.DynamicProxy;
using LogCastle.Abstractions;

namespace LogCastle.Factories
{
    public sealed class ProxyFactory : IProxyFactory
    {
        private readonly IProxyGenerator _proxyGenerator;
        private readonly IInterceptor _interceptor;

        public ProxyFactory(IProxyGenerator proxyGenerator, IInterceptor interceptor)
        {
            _proxyGenerator = proxyGenerator;
            _interceptor = interceptor;
        }

        public TInterface CreateInterfaceProxyWithTarget<TInterface>(TInterface target) where TInterface : class
        {
            return _proxyGenerator.CreateInterfaceProxyWithTarget(target, _interceptor);
        }

        public TClass CreateClassProxyWithTarget<TClass>(TClass target) where TClass : class
        {
            return _proxyGenerator.CreateClassProxyWithTarget(target, _interceptor);
        }

        public object CreateClassProxyWithTarget(Type classToProxy, object target)
        {
            return _proxyGenerator.CreateClassProxyWithTarget(classToProxy, target, _interceptor);
        }

        public object CreateInterfaceProxyWithTarget(Type interfaceToProxy, object target)
        {
            return _proxyGenerator.CreateInterfaceProxyWithTarget(interfaceToProxy, target, _interceptor);
        }

        public object CreateClassProxy(Type classToProxy, object[] constructorArguments)
        {
            return _proxyGenerator.CreateClassProxy(classToProxy, constructorArguments, _interceptor);
        }                
    }
}