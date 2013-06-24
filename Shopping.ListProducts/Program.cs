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

using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Shopping.v1;
using Google.Apis.Shopping.v1.Data;

namespace Shopping.ListProducts
{
    /// <summary>
    /// This sample uses the Shopping API to list a set of products.
    /// http://code.google.com/apis/shopping/search/v1/getting_started.html
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Shopping API: List products");

            // Create the service.
            var service = new ShoppingService(new BaseClientService.Initializer()
                {
                    ApiKey = GetApiKey(),
                    ApplicationName = "Shopping API Sample",
                });
            RunSample(service);
            CommandLine.PressAnyKeyToExit();
        }

        private static string GetApiKey()
        {
            return PromptingClientCredentials.EnsureSimpleClientCredentials().ApiKey;
        }

        static void RunSample(ShoppingService service)
        {
            // Build the request.
            string query = "Camera";
            CommandLine.RequestUserInput("Product to search for", ref query);
            CommandLine.WriteLine();
            CommandLine.WriteAction("Executing request ...");

            var request = service.Products.List("public");
            request.Country = "us";
            request.Q = query;

            // Parse the response.
            long startIndex = 1;
            do
            {
                request.StartIndex = startIndex;
                Products response = request.Execute();

                if (response.CurrentItemCount == 0)
                {
                    break; // Nothing more to list.
                }

                // Show the items.
                foreach (Product item in response.Items)
                {
                    CommandLine.WriteResult((startIndex++) + ". Result", item.ProductValue.Title.TrimByLength(60));
                }
            } while (CommandLine.RequestUserChoice("Do you want to see more items?"));
        }
    }
}
