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
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.SiteVerification.v1;
using Google.Apis.SiteVerification.v1.Data;
using Google.Apis.Util;

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
        private static readonly string Scope = SiteVerificationService.Scopes.Siteverification.GetStringValue();

        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Site Verification sample");

            // Register the authenticator.
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description);
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            provider.ClientIdentifier = credentials.ClientId;
            provider.ClientSecret = credentials.ClientSecret;
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication);

            // Create the service.
            var service = new SiteVerificationService(new BaseClientService.Initializer
                {
                    Authenticator = auth
                });
            RunVerification(service);
            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // Retrieve the authorization from the user.
            return AuthorizationMgr.RequestNativeAuthorization(client, Scope);
        }

        /// <summary>
        /// This method contains the actual sample code.
        /// </summary>
        private static void RunVerification(SiteVerificationService service)
        {
            // Request user input.
            string site = Util.GetSingleLineClipboardContent(96);
            CommandLine.WriteAction("Please enter the URL of the site to verify:");
            CommandLine.RequestUserInput("URL", ref site);
            CommandLine.WriteLine();

            // Example of a GetToken call.
            CommandLine.WriteAction("Retrieving a meta token ...");
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
            CommandLine.WriteResult("Token", response.Token);
            Util.SetClipboard(response.Token);
            CommandLine.WriteLine();

            CommandLine.WriteAction("Please place this token on your webpage now.");
            CommandLine.PressEnterToContinue();
            CommandLine.WriteLine();

            // Example of an Insert call.
            CommandLine.WriteAction("Verifiying...");
            var body = new SiteVerificationWebResourceResource();
            body.Site = new SiteVerificationWebResourceResource.SiteData();
            body.Site.Identifier = site;
            body.Site.Type = "site";
            var verificationResponse = service.WebResource.Insert(body, "meta").Execute();
            CommandLine.WriteResult("Verification", verificationResponse.Id);
            CommandLine.WriteAction("Verification successful!");
        }
    }
}
