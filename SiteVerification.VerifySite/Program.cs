/*
Copyright 2011 Google Inc

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

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.SiteVerification.v1;
using Google.Apis.SiteVerification.v1.Data;
using Google.Apis.Util.Store;

namespace SiteVerification.VerifySite
{
    /// <summary>
    /// This sample goes through the site verification process by first obtaining a token, having the user
    /// add it to the target site, and then inserting the site into the verified owners list. It uses the
    /// SiteVerification API to do so.
    /// 
    /// http://code.google.com/apis/siteverification/v1/getting_started.html
    /// </summary>
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            Console.WriteLine("Site Verification sample");
            Console.WriteLine("========================");

            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { SiteVerificationService.Scope.Siteverification },
                    "user", CancellationToken.None, new FileDataStore("SiteVerification.VerifySite")).Result;
            }

            // Create the service.
            var service = new SiteVerificationService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "SiteVerification API Sample",
                });
            RunVerification(service);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// This method contains the actual sample code.
        /// </summary>
        private static void RunVerification(SiteVerificationService service)
        {
            // Request user input.
            Console.WriteLine("Please enter the URL of the site to verify:");
            var site = Console.ReadLine();
            Console.WriteLine();

            // Example of a GetToken call.
            Console.WriteLine("Retrieving a meta token ...");
            var request = service.WebResource.GetToken(new SiteVerificationWebResourceGettokenRequest()
            {
                VerificationMethod = "meta",
                Site = new SiteVerificationWebResourceGettokenRequest.SiteData()
                {
                    Identifier = site,
                    Type = "site"
                }
            });
            var response = request.Execute();
            Console.WriteLine("Token: " + response.Token);
            Console.WriteLine();

            Console.WriteLine("Please place this token on your webpage now.");
            Console.WriteLine("Press ENTER to continue");
            Console.ReadLine();
            Console.WriteLine();

            // Example of an Insert call.
            Console.WriteLine("Verifying...");
            var body = new SiteVerificationWebResourceResource();
            body.Site = new SiteVerificationWebResourceResource.SiteData();
            body.Site.Identifier = site;
            body.Site.Type = "site";
            var verificationResponse = service.WebResource.Insert(body, "meta").Execute();

            Console.WriteLine("Verification:" + verificationResponse.Id);
            Console.WriteLine("Verification successful!");
        }
    }
}
