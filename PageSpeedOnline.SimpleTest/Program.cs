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
using System.Threading.Tasks;

using Google.Apis.Pagespeedonline.v1;
using Google.Apis.Services;

namespace PageSpeedOnline.SimpleTest
{
    /// <summary>
    /// This sample uses the Page Speed API to run a speed test on the page you specify.
    /// https://code.google.com/apis/pagespeedonline/v1/getting_started.html
    /// </summary>
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Page Speed Online API");
            Console.WriteLine("=====================");

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
            // Create the service.
            var service = new PagespeedonlineService(new BaseClientService.Initializer()
                {
                    ApiKey = GetApiKey(),
                    ApplicationName = "PageSpeedOnline API Sample",
                });

            string url = "http://example.com";
            Console.Write("Enter a URL to search [{0}]: ", url);
            var input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                url = input;
            }

            Console.WriteLine();

            // Run the request.
            Console.WriteLine("Measuring page score ...");
            var result = await service.Pagespeedapi.Runpagespeed(url).ExecuteAsync();

            // Display the results.
            Console.WriteLine("Page score: " + result.Score);
        }

        private static string GetApiKey()
        {
            Console.Write("Enter the API Key: ");
            return Console.ReadLine();
        }
    }
}
