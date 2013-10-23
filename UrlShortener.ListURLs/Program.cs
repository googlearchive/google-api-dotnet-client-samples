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
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;
using Google.Apis.Util.Store;

namespace UrlShortener.ListURLs
{
    /// <summary>
    /// URLShortener OAuth2 Sample. This sample uses OAuth2 to retrieve a list of all the URL's you have shortened so 
    /// far.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("URLShortener - List URLs");
            Console.WriteLine("========================");

            try
            {
                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { UrlshortenerService.Scope.Urlshortener },
                    "user", CancellationToken.None, new FileDataStore("UrlShortener.Auth.Store"));
            }


            // Create the service.
            var service = new UrlshortenerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "UrlShortener.ListURLs Sample",
            });

            // List all shortened URLs:
            Console.WriteLine("Retrieving list of shortened urls...");

            int i = 0;
            string nextPageToken = null;
            do
            {
                // Create and execute the request.
                var request = service.Url.List();
                request.StartToken = nextPageToken;
                UrlHistory result = await request.ExecuteAsync();

                // List all items on this page.
                if (result.Items != null)
                {
                    foreach (Url item in result.Items)
                    {
                        Console.WriteLine((++i) + ") URL" + item.Id + " -> " + item.LongUrl);
                    }
                }

                // Continue with the next page.
                nextPageToken = result.NextPageToken;
            } while (!string.IsNullOrEmpty(nextPageToken));

            if (i == 0)
            {
                Console.WriteLine("You don't have any shortened URLs! Visit http://goo.gl and create some.");
            }
        }
    }
}
