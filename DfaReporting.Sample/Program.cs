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
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Dfareporting.v1_2;
using Google.Apis.Dfareporting.v1_2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace DfaReporting.Sample
{
    /// <summary>
    /// A sample application that runs multiple requests against the DFA Reporting API. These include:
    /// <list type="bullet">
    /// <item>
    /// <description>Listing all DFA user profiles for a user</description>
    /// </item>
    /// <item>
    /// <description>Listing the first 50 available advertisers for a user profile</description>
    /// </item>
    /// <item>
    /// <description>Creating a new standard report</description>
    /// </item>
    /// <item>
    /// <description>Listing the first 50 available Floodlight configuration IDs for a user profile</description>
    /// </item>
    /// <item>
    /// <description>Creating a new shared Floodlight report</description>
    /// </item>
    /// <item>
    /// <description>Generating a new report file from a report</description>
    /// </item>
    /// <item>
    /// <description>Downloading the contents of a report file</description>
    /// </item>
    /// <item>
    /// <description>Listing all available reports</description>
    /// </item>
    /// </list>
    /// </summary>
    internal class Program
    {
        private static readonly IEnumerable<string> scopes = new[] {
            "https://www.googleapis.com/auth/devstorage.read_only",
            DfareportingService.Scope.Dfareporting };

        private const int MaxListPageSize = 50;
        private const int MaxReportPageSize = 10;
        private static readonly DateTime StartDate = DateTime.Today.AddDays(-7);
        private static readonly DateTime EndDate = DateTime.Today;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("DFA Reporting API Command Line Sample");
            Console.WriteLine("=====================================");
            Console.WriteLine("");

            try
            {
                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Run()
        {
            UserCredential credential;
            using (var stream = new System.IO.FileStream("client_secrets.json", System.IO.FileMode.Open,
                System.IO.FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "dfa-user", CancellationToken.None, new FileDataStore("DfaReporting.Sample"));
            }

            // Create the service.
            var service = new DfareportingService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "DFA API Sample",
                });

            // Choose a user profile ID to use in the following samples.
            var userProfileId = GetUserProfileId(service);
            if (!userProfileId.HasValue)
            {
                return;
            }

            // Create and run a standard report.
            CreateAndRunStandardReport(service, userProfileId.Value);

            // Create and run a Floodlight report.
            CreateAndRunFloodlightReport(service, userProfileId.Value);

            // List all of the Reports you have access to.
            new GetAllReportsHelper(service).List(userProfileId.Value, MaxReportPageSize);
        }

        private static long? GetUserProfileId(DfareportingService service)
        {
            UserProfileList userProfiles = new GetAllUserProfilesHelper(service).Run();
            if (userProfiles.Items.Count == 0)
            {
                Console.WriteLine("No user profiles found.");
                return null;
            }
            return userProfiles.Items[0].ProfileId;
        }

        private static void CreateAndRunStandardReport(DfareportingService service, long userProfileId)
        {
            DimensionValueList advertisers = new GetDimensionValuesHelper(service).Query(
                "dfa:advertiser", userProfileId, StartDate, EndDate, MaxListPageSize);

            if (advertisers.Items.Count > 0)
            {
                // Get an advertiser to report on.
                DimensionValue advertiser = advertisers.Items[0];

                Report standardReport = new CreateStandardReportHelper(service).Insert(
                    userProfileId, advertiser, StartDate, EndDate);
                File file = new GenerateReportFileHelper(service).Run(userProfileId, standardReport, true);

                if (file != null)
                {
                    // If the report file generation did not fail, display results.
                    new DownloadReportFileHelper(service).Run(file);
                }
            }
        }

        private static void CreateAndRunFloodlightReport(DfareportingService service, long userProfileId)
        {
            DimensionValueList floodlightConfigIds = new GetDimensionValuesHelper(service).Query(
                "dfa:floodlightConfigId", userProfileId, StartDate, EndDate, MaxListPageSize);

            if (floodlightConfigIds.Items.Count > 0)
            {
                // Get a Floodlight Config ID, so we can run the rest of the samples.
                DimensionValue floodlightConfigId = floodlightConfigIds.Items[0];

                Report floodlightReport = new CreateFloodlightReportHelper(service).Insert(
                    userProfileId, floodlightConfigId, StartDate, EndDate);
                File file = new GenerateReportFileHelper(service).Run(userProfileId, floodlightReport, false);

                if (file != null)
                {
                    // If the report file generation did not fail, display results.
                    new DownloadReportFileHelper(service).Run(file);
                }
            }
        }
    }
}
