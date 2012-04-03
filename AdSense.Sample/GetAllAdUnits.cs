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
    /// This example gets all ad units in an ad client.
    ///
    /// Tags: adunits.list
    /// </summary>
    class GetAllAdUnits
    {
        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="maxPageSize">The maximum page size to retrieve.</param>
        /// <returns>The last page of retrieved accounts.</returns>
        public static AdUnits Run(AdsenseService adsense, string adClientId, int maxPageSize)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all ad units for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdUnits adUnitResponse = null;

            do
            {
                var adUnitRequest = adsense.Adunits.List(adClientId);
                adUnitRequest.MaxResults = maxPageSize;
                adUnitRequest.PageToken = pageToken;
                adUnitResponse = adUnitRequest.Fetch();

                if (adUnitResponse.Items != null && adUnitResponse.Items.Count > 0)
                {
                    foreach (var adUnit in adUnitResponse.Items)
                    {
                        CommandLine.WriteLine("Ad unit with code \"{0}\", name \"{1}\" and status \"{2}\" " +
                            "was found.", adUnit.Code, adUnit.Name, adUnit.Status);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No ad units found.");
                }

                pageToken = adUnitResponse.NextPageToken;

            } while (pageToken != null);

            CommandLine.WriteLine();

            // Return the last page of ad units, so that the main sample has something to run.
            return adUnitResponse;
        }
    }
}
