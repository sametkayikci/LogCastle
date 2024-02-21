using System;
using static System.AttributeTargets;

namespace LogCastle.Attributes
{
    /// <summary>
    /// Belirli bir başlangıç noktasından itibaren belirli bir uzunlukta karakterleri maskeler.
    /// Start ve Length parametreleri ile maskeleme işleminin başlayacağı index ve
    /// maskeleme uzunluğu belirlenir.
    /// </summary>
    /// <example>
    /// Aşağıdaki kullanım, 'password' kelimesinin ilk 2 karakterini maskeler:
    /// [Mask(0, 2)]
    /// public string Password { get; set; }
    /// </example>
    /// <remarks>
    /// Start parametresi 0'dan küçük olamaz ve Length parametresi 1'den büyük olmalıdır.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">start veya length parametreleri belirlenen koşulları sağlamazsa fırlatılır.</exception>
    [AttributeUsage(Parameter | Property | Field)]
    public sealed class MaskAttribute : Attribute
    {
        public int Start { get; private set; }
        public int Length { get; private set; }
    
        /// <param name="start">Maskeleme işleminin başlayacağı index numarası.</param>
        /// <param name="length">Kaç karakterin maskeleneceğini belirten uzunluk.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public MaskAttribute(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Başlangıç değeri negatif olamaz.");

            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Uzunluk değeri sıfırdan büyük olmalıdır.");

            Start = start;
            Length = length;
        }
    }
}