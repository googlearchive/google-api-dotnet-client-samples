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

using Google.Apis.Authentication;
using Google.Apis.Samples.Helper;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;

namespace Google.Apis.Samples.CmdUrlShortener
{
    /// <summary>
    /// This example shows you how to use a CodeGenerated library to access Google APIs. 
    /// In this case the URLShortener service is used to execute simple resolve & get requests.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main method
        /// </summary>
        internal static void Main(string[] args)
        {
            // Initialize this sample
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("URL Shortener");

            // Create the service
            var service = new UrlshortenerService();

            // Ask the user what he wants to do
            CommandLine.RequestUserChoice(
                "What do you want to do?", new UserOption("Create a short URL", () => CreateShortURL(service)),
                new UserOption("Resolve a short URL", () => ResolveShortURL(service)));

            CommandLine.PressAnyKeyToExit();
        }

        private static void ResolveShortURL(UrlshortenerService service)
        {
            // Request input
            string urlToResolve = "http://goo.gl/hcEg7";
            CommandLine.RequestUserInput("URL to resolve", ref urlToResolve);
            CommandLine.WriteLine();

            // Resolve URL
            Url response = service.Url.Get(urlToResolve).Execute();

            // Display response
            CommandLine.WriteLine(" ^1Status:   ^9{0}", response.Status);
            CommandLine.WriteLine(" ^1Long URL: ^9{0}", response.LongUrl);
        }

        private static void CreateShortURL(UrlshortenerService service)
        {
            // Request input
            string urlToShorten = "http://maps.google.com/";
            CommandLine.RequestUserInput("URL to shorten", ref urlToShorten);
            CommandLine.WriteLine();

            // Shorten URL
            Url response = service.Url.Insert(new Url { LongUrl = urlToShorten }).Execute();

            // Display response
            CommandLine.WriteLine(" ^1Short URL: ^9{0}", response.Id);
        }
    }
}