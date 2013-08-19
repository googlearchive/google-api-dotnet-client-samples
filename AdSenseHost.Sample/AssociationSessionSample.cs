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

using Google.Apis.AdSenseHost.v4_1;
using Google.Apis.AdSenseHost.v4_1.Data;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Util;

namespace AdSenseHost.Sample
{
    /// <summary>
    /// A sample application that handles sessions on the AdSense Host API. 
    /// For more information visit the "Host API Signup Flow" guide in
    /// https://developers.google.com/adsense/host/signup
    /// <list type="bullet">
    /// <item>
    /// <description>Starting an association session</description> 
    /// </item>
    /// <item>
    /// <description>Verifying an association session</description>
    /// </item>
    /// </list> 
    /// </summary>
    internal class AssociationSessionSample
    {
        private static readonly string Scope = AdSenseHostService.Scopes.Adsensehost.GetStringValue();

        [STAThread]
        public static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("AdSense Host API Command Line Sample - Association sessions");

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
            AdSenseHostService service = new AdSenseHostService(new BaseClientService.Initializer()
                {
                    Authenticator = auth,
                    ApplicationName = "Adsense Host API Sample",
                });

            string websiteUrl = null;
            CommandLine.RequestUserInput("Insert website URL", ref websiteUrl);

            /*  1. Create the association session. */
            StartAssociationSession(service, websiteUrl);

            /* 2. Use the token to verify the association. */
            string callbackToken = null;
            CommandLine.RequestUserInput("Insert callback token", ref callbackToken);

            VerifyAssociationSession(service, callbackToken);

            CommandLine.PressAnyKeyToExit();
        }

        /// <summary>
        /// This example starts an association session.
        /// </summary>
        /// <param name="adsense">AdSensehost service object on which to run the requests.</param>
        /// <param name="websiteUrl">The URL of the user's hosted website.</param>
        /// <returns>The created association.</returns>
        public static AssociationSession StartAssociationSession(AdSenseHostService adsense, string websiteUrl)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Creating new association session");
            CommandLine.WriteLine("=================================================================");

            // Request a new association session.
            AssociationSession associationSession = adsense.Associationsessions.Start(
                AssociationsessionsResource.StartRequest.ProductCodeEnum.AFC, websiteUrl).Execute();

            CommandLine.WriteLine("Association with ID {0} and redirect URL \n{1}\n was started.",
                associationSession.Id, associationSession.RedirectUrl);

            CommandLine.WriteLine();

            // Return the Association Session that was just created.
            return associationSession;
        }

        /// <summary>
        /// This example verifies an association session callback token.
        /// </summary>
        /// <param name="adsense">AdSensehost service object on which to run the requests.</param>
        /// <param name="callbackToken">The token returned from the association callback.</param>
        public static void VerifyAssociationSession(AdSenseHostService adsense, string callbackToken)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Verifying new association session");
            CommandLine.WriteLine("=================================================================");

            // Verify the association session token.
            AssociationSession associationSession = adsense.Associationsessions.Verify(callbackToken)
                .Execute();

            CommandLine.WriteLine("Association for account {0} has status {1} and ID {2}.",
                associationSession.AccountId, associationSession.Status, associationSession.Id);

            CommandLine.WriteLine();
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.adsensehostsessions";
            const string KEY = "9(R4;.nFt1Kr_y`b'[@d9(R4;.1Kr_y";
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
