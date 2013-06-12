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
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using DotNetOpenAuth.OAuth2;

using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Util;

namespace Google.Apis.Samples.PlusServiceAccount
{
    /// <summary>
    /// This sample demonstrates the simplest use case for a Service Account service.
    /// The certificate needs to be downloaded from the APIs Console
    /// <see cref="https://code.google.com/apis/console/#:access"/>:
    ///   "Create another client ID..." -> "Service Account" -> Download the certificate as "key.p12" and replace the
    ///   placeholder.
    /// The schema provided here can be applied to every request requiring authentication.
    /// <see cref="https://developers.google.com/accounts/docs/OAuth2#serviceaccount"/> for more information.
    /// </summary>
    public class Program
    {
        // A known public activity.
        private static String ACTIVITY_ID = "z12gtjhq3qn2xxl2o224exwiqruvtda0i";

        public static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Plus API - Service Account");

            String serviceAccountEmail = CommandLine.RequestUserInput<String>(
                "Service account e-mail address (from the APIs Console)");

            try
            {
                X509Certificate2 certificate = new X509Certificate2(
                    @"key.p12", "notasecret", X509KeyStorageFlags.Exportable);
                // service account credential (uncomment ServiceAccountUser for domain-wide delegation)
                var provider = new AssertionFlowClient(GoogleAuthenticationServer.Description, certificate)
                {
                    ServiceAccountId = serviceAccountEmail,
                    Scope = PlusService.Scopes.PlusMe.GetStringValue(),
                    // ServiceAccountUser = "user@example.com",
                };
                var auth = new OAuth2Authenticator<AssertionFlowClient>(
                    provider, AssertionFlowClient.GetState);

                // Create the service.
                var service = new PlusService(new BaseClientService.Initializer()
                    {
                        Authenticator = auth
                    });
                Activity activity = service.Activities.Get(ACTIVITY_ID).Execute();
                CommandLine.WriteLine("   ^1Activity: " + activity.Object.Content);
                CommandLine.WriteLine("   ^1Video: " + activity.Object.Attachments[0].Url);
                // Success.
                CommandLine.PressAnyKeyToExit();
            }
            catch (CryptographicException)
            {
                CommandLine.WriteLine(
                    "Unable to load certificate, please download key.p12 file from the Google " +
                    "APIs Console at https://code.google.com/apis/console/");
                CommandLine.PressAnyKeyToExit();
            }

        }
    }
}