using LogCastle.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LogCastle.Caching
{
    public sealed class ControllerBaseMethodCache : IControllerBaseMethodCache
    {
        private readonly Dictionary<Type, HashSet<MethodInfo>> _cache = new Dictionary<Type, HashSet<MethodInfo>>();

        public ControllerBaseMethodCache()
        {
            CacheControllerBaseMethods();
        }

        private void CacheControllerBaseMethods()
        {
            // ControllerBase metotlarını önbelleğe al
            var controllerBaseMethods =
                typeof(ControllerBase).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in controllerBaseMethods)
            {
                var declaringType = method.DeclaringType ?? typeof(ControllerBase);
                if (!_cache.TryGetValue(declaringType, out var methodSet))
                {
                    methodSet = new HashSet<MethodInfo>();
                    _cache[declaringType] = methodSet;
                }
                methodSet.Add(method);
            }
        }

        public bool IsMethodFromControllerBase(MethodInfo methodInfo)
        {
            var declaringType = methodInfo.DeclaringType ?? typeof(ControllerBase);
            return _cache.TryGetValue(declaringType, out var methods) && methods.Contains(methodInfo);
        }
    }
}
