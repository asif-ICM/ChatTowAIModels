using System.Text.RegularExpressions;

namespace Util
{
    public static class Extensions
    {
        public static bool IsNull<T>(this T obj) => obj == null;
        public static bool IsNotNull<T>(this T obj) => obj != null;
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
        public static bool IsNotNullOrEmpty(this string value) => !value.IsNullOrEmpty();
        public static bool IsGreaterThanZero(this int? value) => value.HasValue && value > 0;
        public static bool IsNullOrZero(this int? value) => value.IsNull() || value == 0;
        public static bool IsNullOrZero(this decimal? value) => value.IsNull() || value == 0;
        public static bool IsZero(this int value) => value == 0;
        public static bool IsZero(this decimal value) => value == 0;
        public static bool IsGreaterThanZero(this int value) => value > 0;
        public static bool IsValidList<T>(this IEnumerable<T> Ids) => (Ids != null && Ids.Count() > 0);
        public static bool IsNullOrEmptyList<T>(this IEnumerable<T> Ids) => (Ids == null || Ids.Count() == 0);
        public static string RemoveNonAlphabeticCharacters(this string inputString)
        {
            if (inputString.IsNullOrEmpty())
            {
                return inputString;
            }
            // Regex pattern to match any character that is not a-z or A-Z
            string pattern = "[^a-zA-Z]";

            // Replace non-alphabetic characters with an empty string
            string cleanedString = Regex.Replace(inputString, pattern, "");

            return cleanedString;
        }

    }
}
