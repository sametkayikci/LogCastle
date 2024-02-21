using System.Collections;
using LogCastle.Abstractions;
using LogCastle.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace LogCastle.Logging
{
    public sealed class ObjectResultLoggable : ILoggable
    {
        private readonly ObjectResult _objectResult;
        public ObjectResultLoggable(ObjectResult objectResult)
        {
            _objectResult = objectResult;
        }

        public string ToLogString()
        {
            switch (_objectResult.Value)
            {
                case null:
                    return "null";
                case string stringValue:
                    return stringValue.ToMaskString();
                case IEnumerable enumerable:
                    return enumerable.ToEnumerableString();
                default:
                    return SerializeObjectResponse();
            }
        }
        private string SerializeObjectResponse()
        {
            var isValueTypeOrJObject = _objectResult.Value.GetType().IsValueType;
            var objectResponse = new
            {
                Value = isValueTypeOrJObject
                    ? _objectResult.Value.ToString()
                    : _objectResult.Value.ToMaskedOrSerializedPropertiesLogString(),
                _objectResult.StatusCode
            };

            return isValueTypeOrJObject
                ? _objectResult.Value.ToString()
                : objectResponse.SerializeToJson();
        }
    }
}