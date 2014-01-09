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
using System.Threading;

using Google.Apis.Dfareporting.v1_3;
using Google.Apis.Dfareporting.v1_3.Data;
using Google.Apis.Util;

namespace DfaReporting.Sample
{
    /// <summary>This example generates a report file from a report.</summary>
    internal class GenerateReportFileHelper
    {
        private readonly DfareportingService service;

        /// <summary>Instantiate a helper for generating a new file for a report.</summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        public GenerateReportFileHelper(DfareportingService service)
        {
            this.service = service;
        }

        /// <summary>Requests the generation of a new report file from a given report.</summary>
        /// <param name="userProfileId">The ID number of the DFA user profile to run this request as.</param>
        /// <param name="report">The report to request a new file for.</param>
        /// <returns>The generated report file.</returns>
        public File Run(long userProfileId, Report report, bool isSynchronous)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Generating a report file for report with ID {0}", report.Id);
            Console.WriteLine("=================================================================");

            ReportsResource.RunRequest request = service.Reports.Run(userProfileId, report.Id.Value);
            request.Synchronous = isSynchronous;
            File reportFile = request.Execute();

            Console.WriteLine("Report execution initiated. Checking for completion...");

            reportFile = WaitForReportRunCompletion(service, userProfileId, reportFile);
            if (!reportFile.Status.Equals("REPORT_AVAILABLE"))
            {
                Console.WriteLine("Report file generation failed to finish. Final status is: {0}", reportFile.Status);
                return null;
            }

            Console.WriteLine("Report file with ID \"{0}\" generated.", reportFile.Id);
            Console.WriteLine();
            return reportFile;
        }

        /// <summary>
        /// Waits for a report file to generate by polling for its status using exponential backoff. In the worst case,
        /// there will be 10 attempts to determine if the report is no longer processing.
        /// </summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        /// <param name="userProfileId">The ID number of the DFA user profile to run this request as.</param>
        /// <param name="file">The report file to poll the status of.</param>
        /// <returns>The report file object, either once it is no longer processing or
        ///     once too much time has passed.</returns>
        private static File WaitForReportRunCompletion(DfareportingService service, long userProfileId,
            File file)
        {
            ExponentialBackOff backOff = new ExponentialBackOff();
            TimeSpan interval;

            file = service.Reports.Files.Get(userProfileId, file.ReportId.Value, file.Id.Value).Execute();

            for (int i = 1; i <= backOff.MaxNumOfRetries; i++)
            {
                if (!file.Status.Equals("PROCESSING"))
                {
                    break;
                }

                interval = backOff.GetNextBackOff(i);
                Console.WriteLine("Polling again in {0} seconds.", interval);
                Thread.Sleep(interval);
                file = service.Reports.Files.Get(userProfileId, file.ReportId.Value, file.Id.Value).Execute();
            }
            return file;
        }
    }
}