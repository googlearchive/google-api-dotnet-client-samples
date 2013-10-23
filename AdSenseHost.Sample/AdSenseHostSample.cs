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

using AdSenseHost.Sample.Host;
using AdSenseHost.Sample.Publisher;
using Google.Apis.AdSenseHost.v4_1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace AdSenseHost.Sample
{
    /// <summary>
    /// A sample application that runs multiple requests against the AdSense Host API
    /// <list type="bullet">
    /// <item>
    /// <description>Host calls for your host account</description> 
    /// </item>
    /// <item>
    /// <description>Publisher calls for your publisher's account (needs a Publisher ID)</description> 
    /// </item>
    /// </list> 
    /// </summary>
    internal class AdSenseHostSample
    {
        private static readonly int MaxListPageSize = 50;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("AdSenseHost sample");
            Console.WriteLine("==================");

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

            // Execute Host calls
            HostApiConsumer hostApiConsumer = new HostApiConsumer(service, MaxListPageSize);
            hostApiConsumer.RunCalls();

            // Execute Publisher calls
            PublisherApiConsumer publisherApiConsumer = new PublisherApiConsumer(service, MaxListPageSize);
            publisherApiConsumer.RunCalls();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
