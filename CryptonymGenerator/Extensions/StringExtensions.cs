using System.Globalization;
using System.Linq;

namespace System
{
    public static class StringExtensions
    {
        public static string LettersOnlyAsUpperCase(this string word) =>
            string.Join(string.Empty, word.ToUpperInvariant().ToCharArray().Where(letter => letter is >= 'A' and <= 'Z'));

        public static int ParseInt(this string word) => int.Parse(word, NumberStyles.Any, CultureInfo.InvariantCulture);
    }
}