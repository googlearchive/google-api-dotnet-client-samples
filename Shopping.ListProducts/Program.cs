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
            Console.WriteLine("Shopping API Sample");
            Console.WriteLine("===================");

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
            var service = new ShoppingService(new BaseClientService.Initializer()
            {
                ApiKey = GetApiKey(),
                ApplicationName = "Shopping API Sample",
            });

            // Build the request.
            string query = "Camera";
            Console.Write("Enter a product to search for [{0}]: ", query);
            var input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                query = input;
            }
            Console.WriteLine();
            Console.WriteLine("Executing request ...");

            var request = service.Products.List("public");
            request.Country = "us";
            request.Q = query;

            // Parse the response.
            int startIndex = 1;
            do
            {
                request.StartIndex = startIndex;
                Products response = await request.ExecuteAsync();

                if (response.CurrentItemCount == 0)
                {
                    break; // Nothing more to list.
                }

                // Show the items.
                foreach (Product item in response.Items)
                {
                    Console.WriteLine((startIndex++) + ". Result:" + item.ProductValue.Title);
                }
            } while (CanContinue());
        }

        private static string GetApiKey()
        {
            Console.Write("Enter the API Key: ");
            return Console.ReadLine();
        }

        private bool CanContinue()
        {
            Console.WriteLine("Do you want to see more items? [Enter 'Yes' or 'No']");
            return Console.ReadLine().ToLower() == "yes";
        }
    }
}
