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

using DotNetOpenAuth.OAuth2;

using Google.Apis.Adsense.v1_3;
using Google.Apis.Adsense.v1_3.Data;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Util;

namespace AdSense.Sample
{
    /// <summary>
    /// A sample application that runs multiple requests against the AdSense Management API.
    /// <list type="bullet">
    /// <item>
    /// <description>Registers the authenticator</description> 
    /// </item>
    /// <item>
    /// <description>Creates the service that queries the API</description> 
    /// </item>
    /// <item>
    /// <description>Executes the requests</description> 
    /// </item>
    /// </list> 
    /// </summary>
    internal class AdSenseSample
    {
        private static readonly string Scope = AdSenseService.Scopes.AdsenseReadonly.GetStringValue();
        private static readonly int MaxListPageSize = 50;

        [STAThread]
        internal static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("AdSense Management API Command Line Sample");

            // Register the authenticator.
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            NativeApplicationClient provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
            {
                ClientIdentifier = credentials.ClientId,
                ClientSecret = credentials.ClientSecret
            };
            OAuth2Authenticator<NativeApplicationClient> auth =
                new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication);

            // Create the service.
            var service = new AdSenseService(new BaseClientService.Initializer()
            {
                Authenticator = auth
            });

            // Execute Publisher calls
            ManagementApiConsumer managementApiConsumer = new ManagementApiConsumer(service, MaxListPageSize);
            managementApiConsumer.RunCalls();

            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.adsense";
            const string KEY = "`7X}^}voR4;.1Kr_ynFt";
            IAuthorizationState state = null;

            try
            {
                // Check if there is a cached refresh token available.
                state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                CommandLine.WriteError("Getting Refresh token failed: " + ex.Message);
                CommandLine.WriteLine("Requesting new authorization...");
                state = null;
            }

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
