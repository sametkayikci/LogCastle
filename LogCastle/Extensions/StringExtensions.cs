using LogCastle.Attributes;
using System;
using System.Reflection;
using System.Text;

namespace LogCastle.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Bir string'in belirli bir bölümünü yıldız (*) karakteri ile maskeler.
        /// </summary>
        /// <param name="input">Maskelenmesi gereken orijinal metin.</param>
        /// <param name="start">Maskelenmeye başlanacak index numarası. Eğer bu değer metnin uzunluğundan büyükse veya negatifse, 0 olarak kabul edilir.</param>
        /// <param name="length">Maskelenen karakter sayısı. Eğer bu toplam uzunluk metnin uzunluğunu aşarsa, metnin sonuna kadar maskelenir.</param>
        /// <returns>Belirli bir bölümü maskelenmiş yeni string. Eğer giriş değeri null veya boş ise, giriş değeri olduğu gibi döndürülür.</returns>
        /// <example>
        /// Aşağıdaki örnek "1234567890" metninin ilk 4 karakterini maskeler:
        /// <code>
        /// var maskedString = "1234567890".Mask(0, 4);
        /// // maskedString değeri "****567890" olur.
        /// </code>
        /// </example>
        public static string Mask(this string input, int start, int length)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
                       
            if (start < 0 || start > input.Length) start = 0;
            if (length < 0 || (start + length) > input.Length) length = input.Length - start;

            var builder = new StringBuilder(input);
            for (var i = start; i < Math.Min(start + length, input.Length); i++)
            {
                builder[i] = '*';
            }
            return builder.ToString();
        }

        public static bool IsJsonString(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}")) || // For object
                   (input.StartsWith("[") && input.EndsWith("]"));    // For array
        }
        internal static string ToMaskString(this string value)
        {
            var maskAttribute = value.GetType().GetCustomAttribute<MaskAttribute>();
            return maskAttribute != null ? value.Mask(maskAttribute.Start, maskAttribute.Length) : value;
        }
    }
}
