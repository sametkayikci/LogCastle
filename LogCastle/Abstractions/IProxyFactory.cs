using System;

namespace LogCastle.Abstractions
{
    public interface IProxyFactory
    {
        TInterface CreateInterfaceProxyWithTarget<TInterface>(TInterface target) where TInterface : class;
        TClass CreateClassProxyWithTarget<TClass>(TClass target) where TClass : class;
        object CreateClassProxyWithTarget(Type classToProxy, object target);
        object CreateInterfaceProxyWithTarget(Type interfaceToProxy, object target);
        object CreateClassProxy(Type classToProxy, object[] constructorArguments);
    }
}
