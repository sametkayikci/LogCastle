using System.Text.Json;

namespace LogCastle.Extensions
{
    internal static class JsonExtensions
    {
        internal static string SerializeToJson(this object obj, JsonSerializerOptions options = null)
        {
            if (options is null)
            {
                options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
            }

            return JsonSerializer.Serialize(obj, options);
        }
    }
}
