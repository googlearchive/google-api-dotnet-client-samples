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
using Google.Apis.Adsense.v1_1;
using Google.Apis.Adsense.v1_1.Data;
using Google.Apis.Discovery;
using Google.Apis.Samples.Helper;
using Google.Apis.Util;

namespace AdSense.Sample
{
    /// <summary>
    /// A sample application that runs multiple requests against the AdSense Management API.
    /// These include:
    /// <ul>
    /// <li>Listing all AdSense accounts for a user</li>
    /// <li>Listing the sub-account tree for an account</li>
    /// <li>Listing all ad clients for an account</li>
    /// <li>Listing all ad clients for the default account</li>
    /// <li>Listing all ad units for an ad client</li>
    /// <li>Listing all custom channels for an ad unit</li>
    /// <li>Listing all custom channels for an ad client</li>
    /// <li>Listing all ad units for a custom channel</li>
    /// <li>Listing all URL channels for an ad client</li>
    /// <li>Running a report for an ad client, for the past 7 days</li>
    /// <li>Running a paginated report for an ad client, for the past 7 days</li>
    /// </ul>
    ///
    /// Tags: adclients.list
    /// </summary>
    internal class Program
    {
        private static readonly string Scope = AdsenseService.Scopes.AdsenseReadonly.GetStringValue();
        private static readonly int MaxListPageSize = 50;
        private static readonly int MaxReportPageSize = 50;

        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("AdSense Management API Command Line Sample");

            // Register the authenticator.
            var credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                };
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication);

            // Create the service.
            var service = new AdsenseService(new BaseClientService.Initializer()
                {
                    Authenticator = auth
                });

            var accounts = GetAllAccounts.Run(service, MaxListPageSize);
            if (accounts.Items != null && accounts.Items.Count > 0)
            {
                // Get an example account ID, so we can run the following samples.
                var exampleAccountId = accounts.Items[0].Id;
                GetAccountTree.Run(service, exampleAccountId);
                GetAllAdClientsForAccount.Run(service, exampleAccountId, MaxListPageSize);
            }

            var adClients = GetAllAdClients.run(service, MaxListPageSize);
            if (adClients.Items != null && adClients.Items.Count > 0)
            {
                // Get an ad client ID, so we can run the rest of the samples.
                var exampleAdClientId = adClients.Items[0].Id;

                var adUnits = GetAllAdUnits.Run(service, exampleAdClientId, MaxListPageSize);
                if (adUnits.Items != null && adUnits.Items.Count > 0)
                {
                    // Get an example ad unit ID, so we can run the following sample.
                    var exampleAdUnitId = adUnits.Items[0].Id;
                    GetAllCustomChannelsForAdUnit.Run(service, exampleAdClientId, exampleAdUnitId,
                        MaxListPageSize);
                }

                var customChannels = GetAllCustomChannels.Run(service, exampleAdClientId,
                    MaxListPageSize);
                if (customChannels.Items != null && customChannels.Items.Count > 0)
                {
                    // Get an example custom channel ID, so we can run the following sample.
                    var exampleCustomChannelId = customChannels.Items[0].Id;
                    GetAllAdUnitsForCustomChannel.Run(service, exampleAdClientId, exampleCustomChannelId,
                        MaxListPageSize);
                }

                GetAllUrlChannels.Run(service, exampleAdClientId, MaxListPageSize);
                GenerateReport.Run(service, exampleAdClientId);
                GenerateReportWithPaging.Run(service, exampleAdClientId, MaxReportPageSize);
            }

            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.adsense";
            const string KEY = "`7X}^}voR4;.1Kr_ynFt";

            // Check if there is a cached refresh token available.
            IAuthorizationState state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY);
            if (state != null)
            {
                try
                {
                    client.RefreshToken(state);
                    return state; // Yes - we are done.
                }
                catch (DotNetOpenAuth.Messaging.ProtocolException ex)
                {
                    CommandLine.WriteError("Using existing refresh token failed: " + ex.Message);
                }
            }

            // Retrieve the authorization from the user.
            state = AuthorizationMgr.RequestNativeAuthorization(client, Scope);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
            return state;
        }
    }
}
