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
using Google.Apis.Adsense.v1_1;
using Google.Apis.Adsense.v1_1.Data;
using Google.Apis.Samples.Helper;

namespace AdSense.Sample
{
    /// <summary>
    /// This example retrieves a report, using a filter for a specified ad client.
    ///
    /// Tags: reports.generate
    /// </summary>
    class GenerateReport
    {
        private static readonly string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        public static void Run(AdsenseService adsense, string adClientId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Running report for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Prepare report.
            var startDate = DateTime.Today.ToString(DateFormat);
            var endDate = DateTime.Today.AddDays(-7).ToString(DateFormat);
            var reportRequest = adsense.Reports.Generate(startDate, endDate);

            // Specify the desired ad client using a filter, as well as other parameters.
            reportRequest.Filter = new List<string> { "AD_CLIENT_ID==" + EscapeFilterParameter(adClientId) };
            reportRequest.Metric = new List<string> { "PAGE_VIEWS", "AD_REQUESTS", "AD_REQUESTS_COVERAGE",
                "AD_REQUESTS_CTR", "COST_PER_CLICK", "AD_REQUESTS_RPM", "EARNINGS" };
            reportRequest.Dimension = new List<string> { "DATE" };
            reportRequest.Sort = new List<string> { "+DATE" };

            // Run report.
            var reportResponse = reportRequest.Fetch();

            if (reportResponse.Rows != null && reportResponse.Rows.Count > 0)
            {
                displayHeaders(reportResponse.Headers);
                displayRows(reportResponse.Rows);
            }
            else
            {
                CommandLine.WriteLine("No rows returned.");
            }

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Displays the headers for the report.
        /// </summary>
        /// <param name="headers">The list of headers to be displayed</param>
        public static void displayHeaders(IList<AdsenseReportsGenerateResponse.HeadersData> headers)
        {
            foreach (var header in headers)
            {
                CommandLine.Write("{0, -25}", header.Name);
            }
            CommandLine.WriteLine();
        }

        /// <summary>
        /// Displays a list of rows for the report.
        /// </summary>
        /// <param name="rows">The list of rows to display.</param>
        public static void displayRows(IList<IList<String>> rows)
        {
            foreach (var row in rows)
            {
                foreach (var column in row)
                {
                    CommandLine.Write("{0, -25}", column);
                }
                CommandLine.WriteLine();
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
    }
}
