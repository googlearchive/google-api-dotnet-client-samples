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

using DotNetOpenAuth.OAuth2;

using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Dfareporting.v1_1;
using Google.Apis.Dfareporting.v1_1.Data;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Util;

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
        private static readonly string DfaReportingScope = DfareportingService.Scopes.Dfareporting.GetStringValue();
        private const string DevStorageScopeReadOnly = "https://www.googleapis.com/auth/devstorage.read_only";
        private const int MaxListPageSize = 50;
        private const int MaxReportPageSize = 10;
        private static readonly DateTime StartDate = DateTime.Today.AddDays(-7);
        private static readonly DateTime EndDate = DateTime.Today;

        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("DFA Reporting API Command Line Sample");

            // Register the authenticator.
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                };
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication);

            // Create the service.
            var service = new DfareportingService(new BaseClientService.Initializer
                {
                    Authenticator = auth
                });

            // Choose a user profile ID to use in the following samples.
            string userProfileId = GetUserProfileId(service);

            // Create and run a standard report.
            CreateAndRunStandardReport(service, userProfileId);

            // Create and run a Floodlight report.
            CreateAndRunFloodlightReport(service, userProfileId);

            // List all of the Reports you have access to.
            new GetAllReportsHelper(service).List(userProfileId, MaxReportPageSize);

            CommandLine.PressAnyKeyToExit();
        }

        private static string GetUserProfileId(DfareportingService service)
        {
            UserProfileList userProfiles = new GetAllUserProfilesHelper(service).Run();
            if (userProfiles.Items.Count == 0)
            {
                CommandLine.WriteLine("No user profiles found.");
                CommandLine.PressAnyKeyToExit();
                Environment.Exit(1);
            }
            return userProfiles.Items[0].ProfileId;
        }

        private static void CreateAndRunStandardReport(DfareportingService service, string userProfileId)
        {
            DimensionValueList advertisers = new GetDimensionValuesHelper(service).Query(
                "dfa:advertiser", userProfileId, StartDate, EndDate, MaxListPageSize);

            if (advertisers.Items.Count > 0)
            {
                // Get an advertiser to report on.
                DimensionValue advertiser = advertisers.Items[0];

                Report standardReport = new CreateStandardReportHelper(service).Insert(
                    userProfileId, advertiser, StartDate, EndDate);
                File file = new GenerateReportFileHelper(service).Run(userProfileId, standardReport);

                if (file != null)
                {
                    // If the report file generation did not fail, display results.
                    new DownloadReportFileHelper(service).Run(file);
                }
            }
        }

        private static void CreateAndRunFloodlightReport(DfareportingService service, string userProfileId)
        {
            DimensionValueList floodlightConfigIds = new GetDimensionValuesHelper(service).Query(
                "dfa:floodlightConfigId", userProfileId, StartDate, EndDate, MaxListPageSize);

            if (floodlightConfigIds.Items.Count > 0)
            {
                // Get a Floodlight Config ID, so we can run the rest of the samples.
                DimensionValue floodlightConfigId = floodlightConfigIds.Items[0];

                Report floodlightReport = new CreateFloodlightReportHelper(service).Insert(
                    userProfileId, floodlightConfigId, StartDate, EndDate);
                File file = new GenerateReportFileHelper(service).Run(userProfileId, floodlightReport);

                if (file != null)
                {
                    // If the report file generation did not fail, display results.
                    new DownloadReportFileHelper(service).Run(file);
                }
            }
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.dfareporting";
            const string KEY = "GV^F*(#$:P_NLOn890HC";

            // Check if there is a cached refresh token available.
            IAuthorizationState state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY);
            if (state != null)
            {
                try
                {
                    client.RefreshToken(state);
                    return state;
                    // Yes - we are done.
                }
                catch (DotNetOpenAuth.Messaging.ProtocolException ex)
                {
                    CommandLine.WriteError("Using existing refresh token failed: " + ex.Message);
                }
            }

            // Retrieve the authorization from the user.
            state = AuthorizationMgr.RequestNativeAuthorization(client, DfaReportingScope, DevStorageScopeReadOnly);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
            return state;
        }
    }
}
