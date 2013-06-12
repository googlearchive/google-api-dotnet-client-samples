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
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Samples.Helper;

namespace Discovery.ListAPIs
{
    /// <summary>
    /// This example uses the discovery API to list all APIs in the discovery repository.
    /// http://code.google.com/apis/discovery/v1/using.html
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Discovery API");

            // Create the service.
            var service = new DiscoveryService();
            RunSample(service);
            CommandLine.PressAnyKeyToExit();
        }

        private static void RunSample(DiscoveryService service)
        {
            // Run the request.
            CommandLine.WriteAction("Executing List-request ...");
            var result = service.Apis.List().Execute();

            // Display the results.
            if (result.Items != null)
            {
                foreach (DirectoryList.ItemsData api in result.Items)
                {
                    CommandLine.WriteResult(api.Id, api.Title);
                }
            }
        }
    }
}
