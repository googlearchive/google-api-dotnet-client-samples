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
using System.Threading;

using Google.Apis.Dfareporting.v1_1;
using Google.Apis.Dfareporting.v1_1.Data;

using Google.Apis.Samples.Helper;


namespace DfaReporting.Sample
{
    /// <summary>
    /// This example generates a report file from a report.
    /// </summary>
    internal class GenerateReportFileHelper
    {
        private const int secondsBetweenPolls = 30;
        private readonly DfareportingService service;

        /// <summary>
        /// Instantiate a helper for generating a new file for a report.
        /// </summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        public GenerateReportFileHelper(DfareportingService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Requests the generation of a new report file from a given report.
        /// </summary>
        /// <param name="userProfileId">The ID number of the DFA user profile to run this request as.</param>
        /// <param name="report">The report to request a new file for.</param>
        /// <returns>The generated report file.</returns>
        public File Run(string userProfileId, Report report)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Generating a report file for report with ID {0}", report.Id);
            CommandLine.WriteLine("=================================================================");

            // Run report synchronously.
            ReportsResource.RunRequest request = service.Reports.Run(userProfileId, report.Id);
            request.Synchronous = true;
            File reportFile = request.Fetch();

            CommandLine.WriteLine("Report execution initiated. Checking for completion...");

            reportFile = waitForReportRunCompletion(service, userProfileId, reportFile);

            if (!reportFile.Status.Equals("REPORT_AVAILABLE"))
            {
                CommandLine.WriteLine("Report file generation failed to finish. Final status is: {0}",
                    reportFile.Status);
                return null;
            }

            CommandLine.WriteLine("Report file with ID \"{0}\" generated.", reportFile.Id);
            CommandLine.WriteLine();
            return reportFile;
        }

        /// <summary>
        /// Waits for a report file to generate, checking its status every 30 seconds. Will wait no longer than 5 minutes.
        /// Reports with a lot of data can take longer than 5 minutes to generate.
        /// </summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        /// <param name="userProfileId">The ID number of the DFA user profile to run this request as.</param>
        /// <param name="file">The report file to poll the status of.</param>
        /// <returns>The report file object, either once it is no longer processing or
        ///     once too much time has passed.</returns>
        private static File waitForReportRunCompletion(DfareportingService service, string userProfileId,
            File file)
        {
            TimeSpan timeToSleep = TimeSpan.FromSeconds(secondsBetweenPolls);
            for (int i = 0; i <= 10; i++)
            {
                if (!file.Status.Equals("PROCESSING"))
                {
                    break;
                }
                CommandLine.WriteLine("Polling again in {0} seconds.", secondsBetweenPolls);
                Thread.Sleep(timeToSleep);
                file = service.Reports.Files.Get(userProfileId, file.ReportId, file.Id).Fetch();
            }
            return file;
        }
    }
}
