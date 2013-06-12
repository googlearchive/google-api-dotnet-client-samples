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

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Apis.Adsensehost.v4_1;
using Google.Apis.Adsensehost.v4_1.Data;
using Google.Apis.Samples.Helper;

namespace AdSenseHost.Sample.Host
{
    /// <summary>
    /// A sample consumer that runs multiple Host requests against the AdSense Host API.
    /// These include:
    /// <list type="bullet">
    /// <item>
    /// <description>Getting a list of all host ad clients</description> 
    /// </item>
    /// <item>
    /// <description>Getting a list of all host custom channels</description> 
    /// </item>
    /// <item>
    /// <description>Adding a new host custom channel</description> 
    /// </item>
    /// <item>
    /// <description>Updating an existing host custom channel</description> 
    /// </item>
    /// <item>
    /// <description>Deleting a host custom channel</description> 
    /// </item>
    /// <item>
    /// <description>Getting a list of all host URL channels</description> 
    /// </item>
    /// <item>
    /// <description>Adding a new host URL channel</description> 
    /// </item>
    /// <item>
    /// <description>Deleting an existing host URL channel</description> 
    /// </item>
    /// <item>
    /// <description>Running a report for a host ad client, for the past 7 days</description> 
    /// </item>
    /// </list> 
    /// </summary>
    public class HostApiConsumer
    {
        AdsensehostService service;
        int maxListPageSize;
        private static readonly string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Runs multiple Host requests againt the AdSense Host API.
        /// </summary>
        /// <param name="service">AdSensehost service object on which to run the requests.</param>
        /// <param name="maxListPageSize">The maximum page size to retrieve.</param>
        public HostApiConsumer(AdsensehostService service, int maxListPageSize)
        {
            this.service = service;
            this.maxListPageSize = maxListPageSize;
        }

        internal void RunCalls()
        {
            AdClients adClients = GetAllAdClients();

            // Get a host ad client ID, so we can run the rest of the samples. 
            // Make sure it's a host ad client.
            AdClient exampleAdClient = FindAdClientForHost(adClients.Items);

            if (exampleAdClient != null)
            {
                // Custom Channels: List, Add, Update, Delete
                CustomChannels hostCustomChannels = GetAllCustomChannels(exampleAdClient.Id);
                CustomChannel newCustomChannel = AddCustomChannel(exampleAdClient.Id);
                newCustomChannel = UpdateCustomChannel(exampleAdClient.Id, newCustomChannel.Id);
                DeleteCustomChannel(exampleAdClient.Id, newCustomChannel.Id);

                // URL Channels: List, Add, Delete
                GetAllUrlChannels(exampleAdClient.Id);
                UrlChannel newUrlChannel = AddUrlChannel(exampleAdClient.Id);
                DeleteUrlChannel(exampleAdClient.Id, newUrlChannel.Id);

                GenerateReport(service, exampleAdClient.Id);
            }
            else
            {
                CommandLine.WriteLine("No host ad clients found, unable to run remaining host samples.");
            }
        }

        /// <summary>
        /// This example gets all custom channels in an ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <returns>The last page of custom channels.</returns>
        private CustomChannels GetAllCustomChannels(string adClientId)
        {

            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all custom channels for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve custom channel list in pages and display data as we receive it.
            string pageToken = null;
            CustomChannels customChannelResponse = null;

            do
            {
                var customChannelRequest = this.service.Customchannels.List(adClientId);
                customChannelRequest.MaxResults = this.maxListPageSize;
                customChannelRequest.PageToken = pageToken;
                customChannelResponse = customChannelRequest.Execute();

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

        /// <summary>
        /// This example gets all ad clients for the logged in user's default account.
        /// </summary>
        /// <returns>The last page of ad clients.</returns>
        private AdClients GetAllAdClients()
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all ad clients for default account");
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdClients adClientResponse = null;

            do
            {
                var adClientRequest = this.service.Adclients.List();
                adClientRequest.MaxResults = this.maxListPageSize;
                adClientRequest.PageToken = pageToken;
                adClientResponse = adClientRequest.Execute();

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

        /// <summary>
        /// This example adds a custom channel to a host ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <returns>The created custom channel.</returns>
        private CustomChannel AddCustomChannel(string adClientId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Adding custom channel to ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            CustomChannel newCustomChannel = new CustomChannel();

            System.Random random = new System.Random(System.DateTime.Now.Millisecond);
            newCustomChannel.Name = "Sample Channel #"
                + random.Next(0, 10000).ToString();

            // Create custom channel.
            CustomChannel customChannel = this.service.Customchannels
                .Insert(newCustomChannel, adClientId).Execute();

            CommandLine.WriteLine("Custom channel with id {0}, code {1} and name {2} was created",
                customChannel.Id, customChannel.Code, customChannel.Name);

            CommandLine.WriteLine();

            // Return the Custom Channel that was just created
            return customChannel;
        }

        /// <summary>
        /// This example updates a custom channel on a host ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="customChannelId">The ID for the custom channel to be updated.</param>
        /// <returns>The updated custom channel.</returns>
        private CustomChannel UpdateCustomChannel(string adClientId, string customChannelId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Updating custom channel {0}", customChannelId);
            CommandLine.WriteLine("=================================================================");


            CustomChannel patchCustomChannel = new CustomChannel();

            System.Random random = new System.Random(System.DateTime.Now.Millisecond);
            patchCustomChannel.Name = "Updated Sample Channel #"
                + random.Next(0, 10000).ToString();

            // Update custom channel: Using REST's PATCH method to update just the Name field.
            CustomChannel customChannel = this.service.Customchannels
                .Patch(patchCustomChannel, adClientId, customChannelId).Execute();

            CommandLine.WriteLine("Custom channel with id {0}, code {1} and name {2} was updated",
                customChannel.Id, customChannel.Code, customChannel.Name);

            CommandLine.WriteLine();

            // Return the Custom Channel that was just created
            return customChannel;
        }

        /// <summary>
        /// This example deletes a custom channel on a host ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="customChannelId">The ID for the custom channel to be updated.</param>
        private void DeleteCustomChannel(string adClientId, string customChannelId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Deleting custom channel {0}", customChannelId);
            CommandLine.WriteLine("=================================================================");

            // Delete custom channel
            CustomChannel customChannel = this.service.Customchannels
                .Delete(adClientId, customChannelId).Execute();

            // Delete nonexistent custom channel
            try
            {
                CustomChannel wrongcustomChannel = this.service.Customchannels
                    .Delete(adClientId, "wrong_id").Execute();
            }
            catch (Google.GoogleApiException ex)
            {
                CommandLine.WriteLine("Error with message '{0}' was correctly caught.",
                    ex.Message);
            }


            CommandLine.WriteLine("Custom channel with id {0} was deleted.",
                customChannelId);

            CommandLine.WriteLine();
        }

        /// <summary>
        /// This example gets all URL channels in an host ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        private void GetAllUrlChannels(string adClientId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all URL channels for host ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve URL channel list in pages and display data as we receive it.
            string pageToken = null;
            UrlChannels urlChannelResponse = null;

            do
            {
                var urlChannelRequest = this.service.Urlchannels.List(adClientId);
                urlChannelRequest.MaxResults = this.maxListPageSize;
                urlChannelRequest.PageToken = pageToken;
                urlChannelResponse = urlChannelRequest.Execute();

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

        /// <summary>
        /// This example adds a URL channel to a host ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <returns>The created URL channel.</returns>
        private UrlChannel AddUrlChannel(string adClientId)
        {
            UrlChannel newUrlChannel = new UrlChannel();
            System.Random random = new System.Random(System.DateTime.Now.Millisecond);
            newUrlChannel.UrlPattern = "www.example.com/"
                + random.Next(0, 10000).ToString();

            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Adding URL channel to ad client {0} with pattern {1}", adClientId,
                newUrlChannel.UrlPattern);
            CommandLine.WriteLine("=================================================================");

            // Create URL channel.
            UrlChannel urlChannel = this.service.Urlchannels
                .Insert(newUrlChannel, adClientId).Execute();

            CommandLine.WriteLine("URL channel with id {0} and URL pattern {1} was created",
                urlChannel.Id, urlChannel.UrlPattern);

            CommandLine.WriteLine();

            // Return the URL Channel that was just created
            return urlChannel;
        }

        /// <summary>
        /// This example deletes a URL channel on a host ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="urlChannelId">The ID for the URL channel to be deleted.</param>
        private void DeleteUrlChannel(string adClientId, string urlChannelId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Deleting URL channel {0}", urlChannelId);
            CommandLine.WriteLine("=================================================================");

            // Delete custom channel
            UrlChannel urlChannel = this.service.Urlchannels
                .Delete(adClientId, urlChannelId).Execute();

            CommandLine.WriteLine("Custom channel with id {0} was deleted.",
                urlChannelId);

            CommandLine.WriteLine();
        }

        /// <summary>
        ///  This example prints a report, using a filter for a specified ad client.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        private void GenerateReport(AdsensehostService service, string adClientId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Running report for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Prepare report.
            var startDate = DateTime.Today.ToString(DateFormat);
            var endDate = DateTime.Today.AddDays(-7).ToString(DateFormat);
            ReportsResource.GenerateRequest reportRequest
                = this.service.Reports.Generate(startDate, endDate);

            // Specify the desired ad client using a filter, as well as other parameters.
            // A complete list of metrics and dimensions is available on the documentation.

            reportRequest.Filter = new List<string> { "AD_CLIENT_ID==" 
                + ReportHelper.EscapeFilterParameter(adClientId) };
            reportRequest.Metric = new List<string> { "PAGE_VIEWS", "AD_REQUESTS", "AD_REQUESTS_COVERAGE",
                "AD_REQUESTS_CTR", "COST_PER_CLICK", "AD_REQUESTS_RPM", "EARNINGS" };
            reportRequest.Dimension = new List<string> { "DATE" };

            //A list of dimensions to sort by: + means ascending, - means descending
            reportRequest.Sort = new List<string> { "+DATE" };

            // Run report.
            Report reportResponse = reportRequest.Execute();

            if (reportResponse.Rows != null && reportResponse.Rows.Count > 0)
            {
                ReportHelper.displayHeaders(reportResponse.Headers);
                ReportHelper.displayRows(reportResponse.Rows);
            }
            else
            {
                CommandLine.WriteLine("No rows returned.");
            }

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Finds the first Ad Client whose product code is AFC_HOST.
        /// </summary>
        /// <param name="adClients">List of ad clients.</param>
        /// <returns>Returns the first Ad Client whose product code is AFC_HOST.</returns>
        public static AdClient FindAdClientForHost(IList<AdClient> adClients)
        {
            if (adClients != null)
            {
                return adClients.First(ac => ac.ProductCode == "AFC_HOST");
            }
            return null;
        }
    }

}
