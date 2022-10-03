using System.Text.RegularExpressions;

namespace Domain
{
    public static class StringExtensions
    {
        public static string RemoveEmojis(this string s)
        {
            string text = "x\U0001F310y";
            return Regex.Replace(s, @"\p{Cs}", "");
        }
    }
}
