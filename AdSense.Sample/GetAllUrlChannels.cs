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
    /// This example gets all URL channels in an ad client.
    ///
    /// Tags: urlchannels.list
    /// </summary>
    class GetAllUrlChannels
    {
        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="maxPageSize">The maximum page size to retrieve.</param>
        public static void Run(AdsenseService adsense, string adClientId, int maxPageSize)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all URL channels for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve URL channel list in pages and display data as we receive it.
            string pageToken = null;
            UrlChannels urlChannelResponse = null;

            do
            {
                var urlChannelRequest = adsense.Urlchannels.List(adClientId);
                urlChannelRequest.MaxResults = maxPageSize;
                urlChannelRequest.PageToken = pageToken;
                urlChannelResponse = urlChannelRequest.Fetch();

                if (urlChannelResponse.Items != null && urlChannelResponse.Items.Count > 0)
                {
                    foreach (var urlChannel in urlChannelResponse.Items)
                    {
                        CommandLine.WriteLine("URL channel with pattern \"{0}\" was found.",
                            urlChannel.UrlPattern);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No URL channels found.");
                }

                pageToken = urlChannelResponse.NextPageToken;

            } while (pageToken != null);

            CommandLine.WriteLine();
        }
    }
}
