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

using System.IO;
using System.Net;

using Google.Apis.Dfareporting.v1_1;
using Google.Apis.Dfareporting.v1_1.Data;
using File = Google.Apis.Dfareporting.v1_1.Data.File;

using Google.Apis.Samples.Helper;


namespace DfaReporting.Sample
{
    /// <summary>
    /// This example downloads the contents of a report file.
    /// </summary>
    internal class DownloadReportFileHelper
    {
        private readonly DfareportingService service;

        /// <summary>
        /// Instantiate a helper for downloading the contents of report files.
        /// </summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        public DownloadReportFileHelper(DfareportingService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Fetches the contents of a report file.
        /// </summary>
        /// <param name="reportFile">The completed report file to download.</param>
        public void Run(File reportFile)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Retrieving and printing a report file for report with ID {0}",
                reportFile.ReportId);
            CommandLine.WriteLine("The ID number of this report file is {0}", reportFile.Id);
            CommandLine.WriteLine("=================================================================");

            string url = reportFile.Urls.ApiUrl;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            service.Authenticator.ApplyAuthenticationToRequest(webRequest);
            using (Stream stream = webRequest.GetResponse().GetResponseStream())
            {
                StreamReader reader = new StreamReader(new BufferedStream(stream));

                string report = reader.ReadToEnd();
                CommandLine.WriteLine(report);
            }
        }
    }
}
