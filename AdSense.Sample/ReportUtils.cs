/*
Copyright 2013 Google Inc

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

using Google.Apis.AdSense.v1_4;
using Google.Apis.AdSense.v1_4.Data;

namespace AdSense.Sample
{
    /// <summary>
    /// Collection of utilities to display and modify reports
    /// </summary>
    public static class ReportUtils
    {
        public const string DATEPATTERN = "yyyy-MM-dd";
        public const string MONTHPATTERN = "yyyy-MM";

        /// <summary>
        /// Displays the headers for the report.
        /// </summary>
        /// <param name="headers">The list of headers to be displayed</param>
        public static void DisplayHeaders(IList<AdsenseReportsGenerateResponse.HeadersData> headers)
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
        public static void DisplayRows(IList<IList<string>> rows)
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
        public static string EscapeFilterParameter(string parameter)
        {
            return parameter.Replace("\\", "\\\\").Replace(",", "\\,");
        }

        /// <summary>
        /// Returns a page of results, defined by the request and page parameters.
        /// </summary>
        /// <param name="reportRequest">An instance of the Generate request for the report.</param>
        /// <param name="startIndex">The starting index for this page.</param>
        /// <param name="pageSize">The maximum page size.</param>
        /// <returns>A page of results</returns>
        public static AdsenseReportsGenerateResponse GetPage(
            AccountsResource.ReportsResource.GenerateRequest reportRequest, int startIndex, int pageSize)
        {
            reportRequest.StartIndex = startIndex;
            reportRequest.MaxResults = pageSize;

            // Run next page of report.
            return reportRequest.Execute();
        }

        public static IList<T> NullToEmpty<T>(this IList<T> list)
        {
            return list ?? new List<T>();
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Fetched reports from the API can have chronological gaps. For example, days with no logged activity are 
        /// not shown at all. This method fills day and month gaps and sets the values to "N/A". It doesn't fill 
        /// "WEEK" dimension.
        /// </summary>
        /// <param name="reportResponse">The object containing the report data.</param>
        /// <returns>The full report.</returns>
        public static void FillGapsDates(
            AdsenseReportsGenerateResponse reportResponse, DateTime fromDate, DateTime toDate)
        {
            if (!reportResponse.Rows.IsNullOrEmpty())
            {
                return;
            }

            var enumeratedHeaders = reportResponse.Headers.Select((n, i) => new { header = n, index = i });
            

            var firstDateHeader = enumeratedHeaders.FirstOrDefault(x => x.header.Name == "DATE");
            int dateIndex = firstDateHeader != null ? firstDateHeader.index : -1;

            var firstMonthHeader = enumeratedHeaders.FirstOrDefault(x => x.header.Name == "MONTH");
            int monthIndex = firstMonthHeader != null ? firstMonthHeader.index : -1;

            if (dateIndex == -1 && monthIndex == -1)
            {
                return;
            }
            
            if (reportResponse.Rows == null)
            {
                reportResponse.Rows = new List<IList<string>>();
            }

            // Start date (day) filling.
            if (dateIndex != -1)
            {
                for (DateTime date = fromDate; date.Date <= toDate.Date; date = date.AddDays(1))
                {
                    if (reportResponse.Rows.Any(x => x[dateIndex] == date.ToString(DATEPATTERN)))
                    {
                        continue;
                    }

                    List<string> emptyRow = Enumerable.Repeat("N/A", reportResponse.Headers.Count()).ToList<string>();
                    emptyRow[dateIndex] = date.ToString(DATEPATTERN);

                    // If the result has both days and months, add the month early.
                    if (monthIndex != -1)
                    {
                        emptyRow[monthIndex] = date.ToString(MONTHPATTERN);
                    }

                    reportResponse.Rows.Add(emptyRow);
                }
            }

            // Start month filling.
            if (monthIndex != -1)
            {
                for (DateTime date = fromDate; date.Date <= toDate.Date; date = date.AddMonths(1))
                {
                    // Don't modify rows already in the response.
                    if (reportResponse.Rows.Any(x => x[monthIndex] == date.ToString(MONTHPATTERN)))
                    {
                        continue;
                    }

                    List<string> emptyRow = Enumerable.Repeat("N/A", reportResponse.Headers.Count()).ToList<string>();
                    emptyRow[monthIndex] = date.ToString(MONTHPATTERN);
                    reportResponse.Rows.Add(emptyRow);
                }
            }
        }
    }
}
