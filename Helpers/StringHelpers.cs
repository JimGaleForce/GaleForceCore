using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GaleForceCore.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Indents the specified indent amount.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="indent">The indent.</param>
        /// <returns>System.String.</returns>
        public static string Indent(this string text, int indent) { return string.Empty.PadLeft(indent) + text; }

        /// <summary>
        /// Appends a string if the content is not empty.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="append">The string to append if the value is not empty.</param>
        /// <returns>System.String.</returns>
        public static string AppendIfContent(this string value, string append)
        { return string.IsNullOrWhiteSpace(value) ? value : value + append; }

        /// <summary>
        /// Sanitizes the specified querystring. Quick and dirty method.
        /// </summary>
        /// <param name="querystring">The querystring.</param>
        /// <returns>System.String.</returns>
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

        /// <summary>
        /// Return the text left of other text.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="inclusive">if set to <c>true</c>, includes the separator text.</param>
        /// <returns>System.String.</returns>
        public static string LeftOf(this string source, string separator, bool inclusive = false)
        {
            var index = source.IndexOf(separator);
            if(index == -1)
            {
                return null;
            }

            return source.Substring(0, index + (inclusive ? separator.Length : 0));
        }

        /// <summary>
        /// Return the text right of other text.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="inclusive">if set to <c>true</c>, includes the separator text.</param>
        /// <returns>System.String.</returns>
        public static string RightOf(this string source, string separator, bool inclusive = false)
        {
            var index = source.IndexOf(separator);
            if(index == -1)
            {
                return null;
            }

            return source.Substring(index + (inclusive ? 0 : separator.Length));
        }

        /// <summary>
        /// Return the text right of the last occurence of other text.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="inclusive">if set to <c>true</c>, includes the separator text.</param>
        /// <returns>System.String.</returns>
        public static string RightOfLast(this string source, string separator)
        {
            var index = source.LastIndexOf(separator);
            if(index == -1)
            {
                return null;
            }

            return source.Substring(index + separator.Length);
        }

        /// <summary>
        /// Removes the outside first and last characters.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>System.String[].</returns>
        public static string[] RemoveOutsideChars(this string[] source)
        {
            var items = new string[source.Length];
            for(var i = 0; i < items.Length; i++)
            {
                items[i] = source[i].Length > 2 ? source[i].Substring(1, source[i].Length - 2) : string.Empty;
            }

            return items;
        }

        /// <summary>
        /// Splits the source to return items between left and right separators. Doesn't work for nested items.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>System.String[].</returns>
        public static string[] SplitBy(this string source, string left, string right = null)
        {
            var sets = (((char)255) + source).Split(new[] { left, right }, StringSplitOptions.None);
            var items = new List<string>();
            for(var i = 1; i < sets.Length; i += 2)
            {
                items.Add(sets[i]);
            }

            return items.ToArray();
        }

        /// <summary>
        /// Returns text between a left and right separator.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="leftSeparator">The left separator.</param>
        /// <param name="rightSeparator">The right separator.</param>
        /// <returns>System.String.</returns>
        public static string Between(this string source, string leftSeparator, string rightSeparator = null)
        {
            rightSeparator = rightSeparator ?? leftSeparator;
            return source.RightOf(leftSeparator).LeftOf(rightSeparator);
        }

        /// <summary>
        /// Compares two strings invariant, null friendly.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// <c>true</c> if string are non-null and match, invariantly, <c>false</c> otherwise.
        /// </returns>
        public static bool EqualsInvariant(this string left, string right)
        { return left != null && right != null && left.ToLowerInvariant().Equals(right.ToLowerInvariant()); }

        /// <summary>
        /// Extracts text between to chars.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="startChar">The start character.</param>
        /// <param name="endChar">The end character.</param>
        /// <returns>System.String[].</returns>
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

        /// <summary>
        /// Converts text to Title Case
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.String.</returns>
        public static string ToTitleInvariant(this string text)
        { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text); }
    }
}
