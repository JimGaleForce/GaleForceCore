using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GaleForceCore.Helpers
{
    public static class StringHelper
    {
        public static string Indent(this string text, int indent) { return string.Empty.PadLeft(indent) + text; }

        public static string AppendIfContent(this string value, string append)
        { return string.IsNullOrWhiteSpace(value) ? value : value + append; }

        public static string Sanitize(this string querystring)
        {
            return querystring.Replace("<", string.Empty)
                .Replace(">", string.Empty)
                .Replace("%3c", string.Empty)
                .Replace("\x3c", string.Empty)
                .Replace("%3e", string.Empty)
                .Replace("\x3e", string.Empty)
                .Replace("%3C", string.Empty)
                .Replace("\x3C", string.Empty)
                .Replace("%3E", string.Empty)
                .Replace("\x3E", string.Empty)
                .Trim();
        }

        public static string LeftOf(this string source, string separator, bool inclusive = false)
        {
            var index = source.IndexOf(separator);
            if(index == -1)
            {
                return null;
            }

            return source.Substring(0, index + (inclusive ? separator.Length : 0));
        }

        public static string RightOf(this string source, string separator, bool inclusive = false)
        {
            var index = source.IndexOf(separator);
            if(index == -1)
            {
                return null;
            }

            return source.Substring(index + (inclusive ? 0 : separator.Length));
        }

        public static string RightOfLast(this string source, string separator)
        {
            var index = source.LastIndexOf(separator);
            if(index == -1)
            {
                return null;
            }

            return source.Substring(index + separator.Length);
        }

        public static string[] RemoveQuotes(this string[] source)
        {
            var items = new string[source.Length];
            for(var i = 0; i < items.Length; i++)
            {
                items[i] = source[i].Length > 2 ? source[i].Substring(1, source[i].Length - 2) : string.Empty;
            }

            return items;
        }

        public static string[] SplitBy(this string source, string left, string right = null)
        {
            var sets = ("XXX" + source).Split(new[] { left, right }, StringSplitOptions.None);
            var items = new List<string>();
            for(var i = 1; i < sets.Length; i += 2)
            {
                items.Add(sets[i]);
            }

            return items.ToArray();
        }

        public static string Between(this string source, string leftSeparator, string rightSeparator = null)
        {
            rightSeparator = rightSeparator ?? leftSeparator;
            return source.RightOf(leftSeparator).LeftOf(rightSeparator);
        }

        public static bool EqualsInvariant(this string left, string right)
        { return left != null && right != null && left.ToLowerInvariant().Equals(right.ToLowerInvariant()); }

        public static string[] ExtractBetween(this string text, char startChar, char endChar)
        {
            var inBetween = false;
            var sb = new StringBuilder(text.Length - 2);
            var result = new List<string>();
            foreach(var c in text)
            {
                if(inBetween && c.Equals(endChar))
                {
                    inBetween = false;
                    result.Add(sb.ToString());
                }
                else if(inBetween)
                {
                    sb.Append(c);
                }
                else if(c.Equals(startChar))
                {
                    inBetween = true;
                }
            }

            return result.ToArray();
        }

        public static string ToTitleInvariant(this string text)
        { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text); }
    }
}
