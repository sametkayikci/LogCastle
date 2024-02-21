using System.Reflection;
using LogCastle.Attributes;

namespace LogCastle.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static string ToMaskedOrSerializedPropertyString(this PropertyInfo property, object obj)
        {
            var propValue = property.GetValue(obj, null);
            var maskAttribute = property.GetCustomAttribute<MaskAttribute>();

            if (maskAttribute != null && propValue is string stringValue)
            {
                return $"{property.Name}={stringValue.Mask(maskAttribute.Start, maskAttribute.Length)}";
            }

            if (propValue is string strValue && strValue.IsJsonString())
            {
                return $"{property.Name}={strValue}";
            }

            return $"{property.Name}={propValue.SerializeToJson()}";
        }
    }
}