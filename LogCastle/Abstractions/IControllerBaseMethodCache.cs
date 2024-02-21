using System.Reflection;

namespace LogCastle.Abstractions
{
    public interface IControllerBaseMethodCache
    {
        bool IsMethodFromControllerBase(MethodInfo methodInfo);
    }
}
