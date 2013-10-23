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
using System.IO;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.AdSense.v1_3;
using Google.Apis.AdSense.v1_3.Data;
using Google.Apis.Services;

namespace AdSense.Sample
{
    /// <summary>
    /// A sample application that runs multiple requests against the AdSense Management API.
    /// <list type="bullet">
    /// <item>
    /// <description>Initializes the user credentials</description> 
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
        private static readonly int MaxListPageSize = 50;

        [STAThread]
        internal static void Main(string[] args)
        {
            Console.WriteLine("AdSense Management API Command Line Sample");
            Console.WriteLine("==========================================");

            GoogleWebAuthorizationBroker.Folder = "AdSense.Sample";
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "PUT_CLIENT_ID_HERE",
                    ClientSecret = "PUT_CLIENT_SECRETS_HERE"
                }, new[] { AdSenseService.Scope.AdsenseReadonly }, "user", CancellationToken.None).Result;

            // Create the service.
            var service = new AdSenseService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AdSense Sample"
            });

            // Execute Publisher calls
            ManagementApiConsumer managementApiConsumer = new ManagementApiConsumer(service, MaxListPageSize);
            managementApiConsumer.RunCalls();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
