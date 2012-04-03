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

using Google.Apis.Adsense.v1_1;
using Google.Apis.Adsense.v1_1.Data;
using Google.Apis.Samples.Helper;

namespace AdSense.Sample
{
    /// <summary>
    /// This example gets all ad clients for an account.
    ///
    /// Tags: accounts.adclients.list
    /// </summary>
    class GetAllAdClientsForAccount
    {
        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="accountId">The ID for the account to be used.</param>
        /// <param name="maxPageSize">The maximum page size to retrieve.</param>
        public static void Run(AdsenseService adsense, string accountId, int maxPageSize)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all ad clients for account {0}", accountId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdClients adClientResponse = null;

            do
            {
                var adClientRequest = adsense.Accounts.Adclients.List(accountId);
                adClientRequest.MaxResults = maxPageSize;
                adClientRequest.PageToken = pageToken;
                adClientResponse = adClientRequest.Fetch();

                if (adClientResponse.Items != null && adClientResponse.Items.Count > 0)
                {
                    foreach (var adClient in adClientResponse.Items)
                    {
                        CommandLine.WriteLine("Ad client for product \"{0}\" with ID \"{1}\" was found.",
                            adClient.ProductCode, adClient.Id);
                        CommandLine.WriteLine("\tSupports reporting: {0}",
                            adClient.SupportsReporting.Value ? "Yes" : "No");
                    }
                }
                else
                {
                    CommandLine.WriteLine("No ad clients found.");
                }

                pageToken = adClientResponse.NextPageToken;

            } while (pageToken != null);

            CommandLine.WriteLine();
        }
    }
}
