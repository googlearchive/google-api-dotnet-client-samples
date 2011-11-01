/*
Copyright 2011 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// Extension method container class.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Trims the string to the specified length, and replaces the end with "..." if trimmed.
        /// </summary>
        public static string TrimByLength(this string str, int maxLength)
        {
            if (maxLength < 3)
            {
                throw new ArgumentException("Please specify a maximum length of at least 3", "maxLength");
            }

            if (str.Length <= maxLength)
            {
                return str; // Nothing to do.
            }

            return str.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Formats an Exception as a HTML string.
        /// </summary>
        /// <param name="ex">The exception to format.</param>
        /// <returns>Formatted HTML string.</returns>
        public static string ToHtmlString(this Exception ex)
        {
            string str = ex.ToString();
            str = str.Replace(Environment.NewLine, Environment.NewLine + "<br/>");
            str = str.Replace("  ", " &nbsp;");
            return string.Format("<font color=\"red\">{0}</font>", str);
        }

        /// <summary>
        /// Throws an ArgumentNullException if the specified object is null.
        /// </summary>
        /// <param name="toCheck">The object to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void ThrowIfNull(this object toCheck, string paramName)
        {
            if (toCheck == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if the specified string is null or empty.
        /// </summary>
        /// <param name="toCheck">The object to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void ThrowIfNullOrEmpty(this string toCheck, string paramName)
        {
            if (string.IsNullOrEmpty(toCheck))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if the specified array is null or empty.
        /// </summary>
        /// <param name="toCheck">The object to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void ThrowIfNullOrEmpty(this object[] toCheck, string paramName)
        {
            if (toCheck == null || toCheck.Length == 0)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}