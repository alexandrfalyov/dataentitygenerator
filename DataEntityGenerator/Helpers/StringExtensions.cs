using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataEntityGenerator.Helpers
{
    public static class StringExtensions
    {
        private static readonly Regex UpperWordRegex = new Regex("([A-Z]+[a-z0-9]+)", RegexOptions.Compiled);

        public static string ToCamelCase(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;
            return char.ToLower(source[0]) + source.Substring(1);
        }

        public static string ToDashName(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;
            var matches = UpperWordRegex.Matches(source);
            if (matches.Count > 0)
            {
                return string.Join("-", matches.Cast<Match>().Select(x => x.Groups[1].Value));
            }

            return source;
        }
    }
}