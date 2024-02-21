using Castle.DynamicProxy;
using LogCastle.Attributes;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LogCastle.Extensions
{
    public static class InvocationExtensions
    {
        /// <summary>
        /// Önce metot üzerindeki, sonra sınıf üzerindeki ve son olarak arayüz üzerindeki özellikleri kontrol eder.
        /// </summary>
        /// <param name="invocation">Genişletme metodu olarak eklenen IInvocation nesnesi.</param>
        /// <returns>Verilen invocation için LogCastleAttribute'u örneği veya null.</returns>
        public static LogCastleAttribute GetLogCastleAttribute(this IInvocation invocation)
        {
            var methodAttribute = invocation.MethodInvocationTarget.GetCustomAttribute<LogCastleAttribute>();
            if (methodAttribute != null) return methodAttribute;

            var classAttribute = invocation.TargetType.GetCustomAttribute<LogCastleAttribute>();
            if (classAttribute != null) return classAttribute;

            foreach (var interfaceType in invocation.TargetType.GetInterfaces())
            {
                var map = invocation.TargetType.GetInterfaceMap(interfaceType);
                for (var i = 0; i < map.TargetMethods.Length; i++)
                {
                    if (map.TargetMethods[i] != invocation.MethodInvocationTarget) continue;
                    var interfaceMethodAttribute = map.InterfaceMethods[i].GetCustomAttribute<LogCastleAttribute>();
                    if (interfaceMethodAttribute != null) return interfaceMethodAttribute;
                }
            }

            foreach (var interfaceType in invocation.TargetType.GetInterfaces())
            {
                var interfaceAttribute = interfaceType.GetCustomAttribute<LogCastleAttribute>();
                if (interfaceAttribute != null) return interfaceAttribute;
            }

            return null;
        }

             
        public static string GetFullMethodName(this IInvocation invocation)
        {
            var methodName = invocation.Method.Name;
            var returnType = invocation.Method.ReturnType.Name;
            var parameterInfo = string.Join(", ", invocation.Method.GetParameters()
                                            .Select(p => $"{p.ParameterType.Name} {p.Name}"));
            var methodInfo = $"{returnType}.{methodName}({parameterInfo})";
            return $"{invocation.TargetType.FullName}.{methodInfo}";
        }

        public static string GetMaskedArguments(this IInvocation invocation)
        {
            var logMessage = new StringBuilder();
            var parameters = invocation.Method.GetParameters();
            var arguments = invocation.Arguments;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var argument = arguments[i]; 
                var attribute = parameter.GetCustomAttribute<MaskAttribute>();

                if (attribute != null && argument is string stringValue)
                {
                    logMessage.Append($"{parameter.Name}={stringValue.Mask(attribute.Start, attribute.Length)}");
                }
                else
                {
                    logMessage.Append($"{parameter.Name}={argument?.ToDetailedLogString() ?? "null"}");
                }
                if (i < parameters.Length - 1)
                {
                    logMessage.Append(", ");
                }
            }
            return logMessage.ToString();
        }      
    }
}
