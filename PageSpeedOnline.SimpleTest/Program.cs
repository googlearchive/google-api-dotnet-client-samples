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
using Google.Apis.Pagespeedonline.v1;
using Google.Apis.Samples.Helper;

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
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Page Speed Online API");

            // Create the service.

            var service = new PagespeedonlineService() { Key = GetApiKey() };
            RunSample(service);
            CommandLine.PressAnyKeyToExit();
        }

        private static void RunSample(PagespeedonlineService service)
        {
            string url = "http://example.com";
            CommandLine.RequestUserInput("URL to test", ref url);
            CommandLine.WriteLine();

            // Run the request.
            CommandLine.WriteAction("Measuring page score ...");
            var result = service.Pagespeedapi.Runpagespeed(url).Fetch(); 
            
            // Display the results.
            CommandLine.WriteResult("Page score", result.Score);
        }

        private static string GetApiKey()
        {
            return PromptingClientCredentials.EnsureSimpleClientCredentials().ApiKey;
        }
    }
}
