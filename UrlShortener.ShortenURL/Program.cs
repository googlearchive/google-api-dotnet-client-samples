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

using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;
using Google.Apis.Services;

namespace Google.Apis.Samples.CmdUrlShortener
{
    /// <summary>
    /// This example shows you how to use a CodeGenerated library to access Google APIs. 
    /// In this case the URLShortener service is used to execute simple resolve & get requests.
    /// </summary>
    internal class Program
    {
        private readonly UrlshortenerService service;

        internal static void Main(string[] args)
        {
            Console.WriteLine("URL Shortener Sample");
            Console.WriteLine("====================");

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

        public Program()
        {
            service = new UrlshortenerService(new BaseClientService.Initializer
                {
                    ApplicationName = "UrlShortener.ShortenURL sample",
                });
        }

        private async Task Run()
        {
            Console.WriteLine("What do you want to do?");
            Console.WriteLine("Press 1 to create a short URL");
            Console.WriteLine("Press 2 to resolve a short URL");

            var input = Console.ReadLine();
            if (input == "1")
            {
                await CreateShortURL();
            }
            else if (input == "2")
            {
                await ResolveShortURL();
            }
            else
            {
                Console.WriteLine("Invalid option!");
            }
        }

        private async Task ResolveShortURL()
        {
            // Request input
            string urlToResolve = "http://goo.gl/hcEg7";
            Console.Write("\tEnter a URL to resolve [{0}]: ", urlToResolve);
            var input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                urlToResolve = input;
            }
            Console.WriteLine();

            // Resolve URL
            Url response = await service.Url.Get(urlToResolve).ExecuteAsync();

            // Display response
            Console.WriteLine("\tStatus:  {0}", response.Status);
            Console.WriteLine("\tLong URL:{0}", response.LongUrl);
        }

        private async Task CreateShortURL()
        {
            // Request input
            string urlToShorten = "http://maps.google.com/";
            Console.Write("\tEnter a URL to shorten[{0}]: ", urlToShorten);
            var input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                urlToShorten = input;
            }
            Console.WriteLine();

            // Shorten URL
            Url response = await service.Url.Insert(new Url { LongUrl = urlToShorten }).ExecuteAsync();

            // Display response
            Console.WriteLine("\tShort URL:{0}", response.Id);
        }
    }
}