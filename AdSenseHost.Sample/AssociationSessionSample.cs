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
using System.IO;
using System.Threading;

using Google.Apis.AdSenseHost.v4_1;
using Google.Apis.AdSenseHost.v4_1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;

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
        [STAThread]
        public static void Main(string[] args)
        {
            Console.WriteLine("AdSense Host API Command Line Sample - Association sessions");

            GoogleWebAuthorizationBroker.Folder = "AdSenseHost.Sample";
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { AdSenseHostService.Scope.Adsensehost },
                    "user", CancellationToken.None, new FileDataStore("AdSenseHostSampleStore")).Result;
            }

            // Create the service.
            var service = new AdSenseHostService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AdSense API Sample",
            });

            Console.WriteLine("Insert website URL");
            string websiteUrl = Console.ReadLine();

            /*  1. Create the association session. */
            StartAssociationSession(service, websiteUrl);

            /* 2. Use the token to verify the association. */
            Console.WriteLine("Insert callback token");
            string callbackToken = Console.ReadLine();

            VerifyAssociationSession(service, callbackToken);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>This example starts an association session.</summary>
        /// <param name="adsense">AdSensehost service object on which to run the requests.</param>
        /// <param name="websiteUrl">The URL of the user's hosted website.</param>
        /// <returns>The created association.</returns>
        public static AssociationSession StartAssociationSession(AdSenseHostService adsense, string websiteUrl)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Creating new association session");
            Console.WriteLine("=================================================================");

            // Request a new association session.
            AssociationSession associationSession = adsense.Associationsessions.Start(
                AssociationsessionsResource.StartRequest.ProductCodeEnum.AFC, websiteUrl).Execute();

            Console.WriteLine("Association with ID {0} and redirect URL \n{1}\n was started.",
                associationSession.Id, associationSession.RedirectUrl);

            Console.WriteLine();

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
            Console.WriteLine("=================================================================");
            Console.WriteLine("Verifying new association session");
            Console.WriteLine("=================================================================");

            // Verify the association session token.
            AssociationSession associationSession = adsense.Associationsessions.Verify(callbackToken)
                .Execute();

            Console.WriteLine("Association for account {0} has status {1} and ID {2}.",
                associationSession.AccountId, associationSession.Status, associationSession.Id);

            Console.WriteLine();
        }
    }
}
