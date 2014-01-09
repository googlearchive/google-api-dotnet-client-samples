/*
Copyright 2014 Google Inc

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

using Google.Apis.Dfareporting.v1_3;
using Google.Apis.Dfareporting.v1_3.Data;

namespace DfaReporting.Sample
{
    /// <summary>
    /// This example gets all reports available to the given user profile.
    /// </summary>
    internal class GetAllReportsHelper
    {
        private readonly DfareportingService service;

        /// <summary>
        /// Instantiate a helper for listing all reports available.
        /// </summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        public GetAllReportsHelper(DfareportingService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Lists all available reports for the given user profile.
        /// </summary>
        /// <param name="userProfileId">The ID number of the DFA user profile to run this request as.</param>
        /// <param name="maxPageSize">The maximum number of results per page.</param>
        public void List(long userProfileId, int maxPageSize)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all reports");
            Console.WriteLine("=================================================================");

            // Retrieve account list in pages and display data as we receive it.
            string pageToken = null;
            ReportList reports = null;
            do
            {
                var request = service.Reports.List(userProfileId);
                request.MaxResults = maxPageSize;
                request.PageToken = pageToken;
                reports = request.Execute();

                foreach (var report in reports.Items)
                {
                    Console.WriteLine("Report with ID \"{0}\" and display name \"{1}\" was found.", report.Id,
                        report.Name);
                }

                pageToken = reports.NextPageToken;
            } while (reports.Items.Count > 0);

            Console.WriteLine();
        }
    }
}
