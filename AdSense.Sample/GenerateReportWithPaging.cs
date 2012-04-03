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
    /// This example retrieves a report for a specified ad client, using pagination.
    ///
    /// Please only use pagination if your application requires it due to memory or storage constraints.
    /// If you need to retrieve more than 5000 rows, please check GenerateReport.java, as due to current
    /// limitations you will not be able to use paging for large reports.
    ///
    /// Tags: reports.generate
    /// </summary>
    class GenerateReportWithPaging
    {
        private static readonly int RowLimit = 5000;
        private static readonly string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="maxReportPageSize">The maximum page size to retrieve.</param>
        public static void Run(AdsenseService adsense, string adClientId, int maxReportPageSize)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Running paginated report for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Prepare report.
            var startDate = DateTime.Today.ToString(DateFormat);
            var endDate = DateTime.Today.AddDays(-7).ToString(DateFormat);
            var reportRequest = adsense.Reports.Generate(startDate, endDate);
            var pageSize = maxReportPageSize;
            var startIndex = 0;

            // Specify the desired ad client using a filter, as well as other parameters.
            reportRequest.Filter = new List<string> { "AD_CLIENT_ID==" + 
                GenerateReport.EscapeFilterParameter(adClientId) };
            reportRequest.Metric = new List<string> { "PAGE_VIEWS", "AD_REQUESTS", "AD_REQUESTS_COVERAGE",
                "AD_REQUESTS_CTR", "COST_PER_CLICK", "AD_REQUESTS_RPM", "EARNINGS" };
            reportRequest.Dimension = new List<string> { "DATE" };
            reportRequest.Sort = new List<string> { "+DATE" };

            // Run first page of report.
            var reportResponse = getPage(reportRequest, startIndex, pageSize);

            if (reportResponse.Rows == null || reportResponse.Rows.Count == 0)
            {
                CommandLine.WriteLine("No rows returned.");
                return;
            }

            // Display headers.
            GenerateReport.displayHeaders(reportResponse.Headers);

            // Display first page of results.
            GenerateReport.displayRows(reportResponse.Rows);

            var totalRows = Math.Min(int.Parse(reportResponse.TotalMatchedRows), RowLimit);
            for (startIndex = reportResponse.Rows.Count; startIndex < totalRows;
                startIndex += reportResponse.Rows.Count)
            {
                // Check to see if we're going to go above the limit and get as many results as we can.
                pageSize = Math.Min(maxReportPageSize, totalRows - startIndex);

                // Run next page of report.
                reportResponse = getPage(reportRequest, startIndex, pageSize);

                // If the report size changes in between paged requests, the result may be empty.
                if (reportResponse.Rows == null || reportResponse.Rows.Count == 0)
                {
                    break;
                }

                // Display results.
                GenerateReport.displayRows(reportResponse.Rows);
            }

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Returns a page of results, defined by the request and page parameters.
        /// </summary>
        /// <param name="reportRequest">An instance of the Generate request for the report.</param>
        /// <param name="startIndex">The starting index for this page.</param>
        /// <param name="pageSize">The maximum page size.</param>
        /// <returns></returns>
        private static AdsenseReportsGenerateResponse getPage(
            ReportsResource.GenerateRequest reportRequest, int startIndex, int pageSize)
        {
            reportRequest.StartIndex = startIndex;
            reportRequest.MaxResults = pageSize;

            // Run next page of report.
            return reportRequest.Fetch();
        }
    }
}
