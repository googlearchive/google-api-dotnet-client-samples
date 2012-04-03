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
    /// This example gets all ad clients for the logged in user's default account.
    ///
    /// Tags: adclients.list
    /// </summary>
    class GetAllAdClients
    {
        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="maxPageSize">The maximum page size to retrieve.</param>
        /// <returns>The last page of retrieved accounts.</returns>
        public static AdClients run(AdsenseService adsense, int maxPageSize)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all ad clients for default account");
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdClients adClientResponse = null;

            do
            {
                var adClientRequest = adsense.Adclients.List();
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

            // Return the last page of ad clients, so that the main sample has something to run.
            return adClientResponse;
        }
    }
}
