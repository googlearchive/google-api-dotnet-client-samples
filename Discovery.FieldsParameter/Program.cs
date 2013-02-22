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

namespace Discovery.FieldsParameter
{
    /// <summary>
    /// This example demonstrates how to do a Partial GET using field parameters.
    /// http://code.google.com/apis/discovery/v1/using.html
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Discovery API -- 'Fields'-Parameter");

            // Create the service.
            var service = new DiscoveryService();
            RunSample(service);
            CommandLine.PressAnyKeyToExit();
        }

        private static void RunSample(DiscoveryService service)
        {
            // Run the request.
            CommandLine.WriteAction("Executing Partial GET ...");
            var request = service.Apis.GetRest("discovery", "v1");
            request.Fields = "description,title";
            var result = request.Fetch();

            // Display the results.
            CommandLine.WriteResult("Description", result.Description);
            CommandLine.WriteResult("Title", result.Title);
            CommandLine.WriteResult("Name (not requested)", result.Name);
        }
    }
}
