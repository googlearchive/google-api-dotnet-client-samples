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
    /// This example gets all custom channels in an ad client.
    ///
    /// Tags: customchannels.list
    /// </summary>
    class GetAllCustomChannels
    {
        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="maxPageSize">The maximum page size to retrieve.</param>
        /// <returns>The last page of custom channels.</returns>
        public static CustomChannels Run(AdsenseService adsense, string adClientId, int maxPageSize)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all custom channels for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve custom channel list in pages and display data as we receive it.
            string pageToken = null;
            CustomChannels customChannelResponse = null;

            do
            {
                var customChannelRequest = adsense.Customchannels.List(adClientId);
                customChannelRequest.MaxResults = maxPageSize;
                customChannelRequest.PageToken = pageToken;
                customChannelResponse = customChannelRequest.Fetch();

                if (customChannelResponse.Items != null && customChannelResponse.Items.Count > 0)
                {
                    foreach (var customChannel in customChannelResponse.Items)
                    {
                        CommandLine.WriteLine("Custom channel with code \"{0}\" and name \"{1}\" was found.",
                            customChannel.Code, customChannel.Name);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No custom channels found.");
                }

                pageToken = customChannelResponse.NextPageToken;

            } while (pageToken != null);

            CommandLine.WriteLine();

            // Return the last page of custom channels, so that the main sample has something to run.
            return customChannelResponse;
        }
    }
}
