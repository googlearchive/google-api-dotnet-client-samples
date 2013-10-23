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
using System.Security.Cryptography.X509Certificates;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Services;

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
            Console.WriteLine("Plus API - Service Account");
            Console.WriteLine("==========================");

            String serviceAccountEmail = "SERVICE_ACCOUNT_EMAIL_HERE";

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail)
               {
                   Scopes = new[] { PlusService.Scope.PlusMe }
               }.FromCertificate(certificate));

            // Create the service.
            var service = new PlusService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Plus API Sample",
            });

            Activity activity = service.Activities.Get(ACTIVITY_ID).Execute();
            Console.WriteLine("  Activity: " + activity.Object.Content);
            Console.WriteLine("  Video: " + activity.Object.Attachments[0].Url);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}