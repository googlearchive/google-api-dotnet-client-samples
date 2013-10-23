/*
Copyright 2012 Google Inc

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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Google.Apis.AdSenseHost.v4_1.Data;

namespace AdSenseHost.Sample
{
    internal class ReportHelper
    {
        /// <summary>
        /// Displays the headers for the report.
        /// </summary>
        /// <param name="headers">The list of headers to be displayed</param>
        internal static void displayHeaders(IList<Report.HeadersData> headers)
        {
            foreach (var header in headers)
            {
                Console.WriteLine("{0, -25}", header.Name);
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Displays a list of rows for the report.
        /// </summary>
        /// <param name="rows">The list of rows to display.</param>
        internal static void displayRows(IList<IList<String>> rows)
        {
            foreach (var row in rows)
            {
                foreach (var column in row)
                {
                    Console.WriteLine("{0, -25}", column);
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Escape special characters for a parameter being used in a filter.
        /// </summary>
        /// <param name="parameter">The parameter to be escaped.</param>
        /// <returns>The escaped parameter.</returns>
        internal static string EscapeFilterParameter(string parameter)
        {
            return parameter.Replace("\\", "\\\\").Replace(",", "\\,");
        }
    }
}
