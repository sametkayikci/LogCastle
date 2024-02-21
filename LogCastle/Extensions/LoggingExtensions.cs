using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogCastle.Abstractions;
using LogCastle.Logging;

namespace LogCastle.Extensions
{
    internal static class LoggingExtensions
    {
        /// <summary>
        /// Belirli bir nesnenin detaylı log string temsilini döndürür.
        /// </summary>
        /// <remarks>
        /// Bu metot aşağıdaki tiplere göre özelleştirilmiş loglama işlemleri gerçekleştirir:
        /// - Null ise "null" döndürür.
        /// - String tipinde ise, eğer hassas veri içeriyorsa maskeler.
        /// - ValueType tipinde ise, nesnenin string temsilini döndürür.
        /// - IEnumerable tipinde ise, koleksiyonun elemanlarını temsil eden bir string döndürür.
        /// - Task tipinde ve tamamlanmış ise, Task'in sonucunu döndürür.
        /// - Diğer nesne tipleri için, nesnenin özelliklerini serileştirerek bir JSON string'i oluşturur.
        /// </remarks>
        /// <param name="argument">Log string'ine dönüştürülecek nesne.</param>
        /// <returns>Nesnenin loglaması için uygun string temsilini geri döndürür.</returns>
        internal static string ToDetailedLogString(this object argument)
        {
            ILoggable loggable;
            switch (argument)
            {
                case null:
                    loggable = new NullLoggable();
                    break;
                case string stringValue:
                    loggable = new StringLoggable(stringValue);
                    break;
                case ValueType valueType:
                    loggable = new ValueTypeLoggable(valueType);
                    break;
                case IEnumerable enumerable:
                    loggable = new EnumerableLoggable(enumerable);
                    break;
                case ObjectResult objectResult:
                    loggable = new ObjectResultLoggable(objectResult);
                    break;
                case Task task:
                    loggable = new TaskLoggable(task);
                    break;
                default:
                    loggable = new DefaultLoggable(argument);
                    break;
            }

            return loggable.ToLogString();
        }
             
        internal static string ToEnumerableString(this IEnumerable enumerable)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            var first = true;
            foreach (var item in enumerable)
            {
                if (!first) sb.Append(", ");
                sb.Append(item.ToDetailedLogString());
                first = false;
            }
            sb.Append(']');
            return sb.ToString();
        }

        internal static string ToMaskedOrSerializedPropertiesLogString(this object obj)
        {
            if (obj is null) return "null";

            var properties = obj.GetType().GetProperties()
                                 .Where(prop => prop.CanRead)
                                 .Select(prop => prop.ToMaskedOrSerializedPropertyString(obj));

            return $"{{{string.Join(", ", properties)}}}";
        }
    }
}
