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

using Google.Apis.AdSenseHost.v4_1;
using Google.Apis.AdSenseHost.v4_1.Data;

namespace AdSenseHost.Sample.Publisher
{
    /// <summary>
    /// A sample consumer that runs multiple Host requests against the AdSense Host API.
    /// These include:
    /// <list type="bullet">
    /// <item>
    /// <description>Getting a list of all publisher ad clients</description> 
    /// </item>
    /// <item>
    /// <description>Getting a list of all publisher ad units</description> 
    /// </item>
    /// <item>
    /// <description>Adding a new ad unit</description> 
    /// </item>
    /// <item>
    /// <description>Updating an existing ad unit</description> 
    /// </item>
    /// <item>
    /// <description>Deleting an ad unit</description> 
    /// </item>
    /// <item>
    /// <description>Running a report for a publisher ad client, for the past 7 days</description> 
    /// </item>
    /// </list> 
    /// </summary>
    public class PublisherApiConsumer
    {
        AdSenseHostService service;
        int maxListPageSize;
        private static readonly string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Runs multiple Publisher requests against the AdSense Host API.
        /// </summary>
        /// <param name="service">AdSensehost service object on which to run the requests.</param>
        /// <param name="maxListPageSize">The maximum page size to retrieve.</param>
        public PublisherApiConsumer(AdSenseHostService service, int maxListPageSize)
        {
            this.service = service;
            this.maxListPageSize = maxListPageSize;
        }

        internal void RunCalls()
        {
            Console.WriteLine("For the rest of the samples you'll need a Publisher ID. If you haven't associated an " +
                "AdSense account to your Host, set AssociationSession.cs as startup object and rebuild.");

            // Get publisher ID from user.
            Console.WriteLine("Insert Publisher ID");
            string publisherId = Console.ReadLine();
            if (string.IsNullOrEmpty(publisherId))
            {
                return;
            }

            AdClients publisherAdClients = GetAllAdClients(publisherId);

            if (publisherAdClients.Items != null && publisherAdClients.Items.Count > 0)
            {
                // Get a host ad client ID, so we can run the rest of the samples.
                string examplePublisherAdClientId = publisherAdClients.Items[0].Id;

                GetAllAdUnits(publisherId, examplePublisherAdClientId);
                AdUnit adUnit = AddAdUnit(publisherId,
                    examplePublisherAdClientId);
                adUnit = UpdateAdUnit(publisherId, examplePublisherAdClientId, adUnit.Id);
                DeleteAdUnit(publisherId, examplePublisherAdClientId, adUnit.Id);
                GenerateReport(publisherId, examplePublisherAdClientId);
            }
        }

        /// <summary>
        /// This example gets all ad clients for a publisher account.
        /// </summary>
        /// <param name="accountId">The ID for the publisher's account to be used.</param>
        /// <returns>The last page of retrieved ad clients.</returns>
        private AdClients GetAllAdClients(string accountId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all ad clients for account {0}", accountId);
            Console.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdClients adClientResponse = null;

            do
            {
                var adClientRequest = this.service.Accounts.Adclients.List(accountId);
                adClientRequest.MaxResults = this.maxListPageSize;
                adClientRequest.PageToken = pageToken;
                adClientResponse = adClientRequest.Execute();

                if (adClientResponse.Items != null && adClientResponse.Items.Count > 0)
                {
                    foreach (var adClient in adClientResponse.Items)
                    {
                        Console.WriteLine("Ad client for product \"{0}\" with ID \"{1}\" was found.",
                            adClient.ProductCode, adClient.Id);
                        Console.WriteLine("\tSupports reporting: {0}",
                            adClient.SupportsReporting.Value ? "Yes" : "No");
                    }
                }
                else
                {
                    Console.WriteLine("No ad clients found.");
                }

                pageToken = adClientResponse.NextPageToken;

            } while (pageToken != null);
            Console.WriteLine();

            // Return the last page of ad clients, so that the main sample has something to run.
            return adClientResponse;
        }

        /// <summary>This example prints all ad units in a publisher ad client.</summary>
        /// <param name="accountId">The ID for the publisher account to be used.</param>
        /// <param name="adClientId">An arbitrary publisher ad client ID.</param>
        private void GetAllAdUnits(string accountId, string adClientId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all ad units for ad client {0}", adClientId);
            Console.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdUnits adUnitResponse = null;

            do
            {
                var adUnitRequest = this.service.Accounts.Adunits.List(accountId, adClientId);
                adUnitRequest.MaxResults = this.maxListPageSize;
                adUnitRequest.PageToken = pageToken;
                adUnitResponse = adUnitRequest.Execute();

                if (adUnitResponse.Items != null && adUnitResponse.Items.Count > 0)
                {
                    foreach (var adUnit in adUnitResponse.Items)
                    {
                        Console.WriteLine("Ad unit with code \"{0}\", name \"{1}\" and status \"{2}\" " +
                            "was found.", adUnit.Code, adUnit.Name, adUnit.Status);
                    }
                }
                else
                {
                    Console.WriteLine("No ad units found.");
                }

                pageToken = adUnitResponse.NextPageToken;
            } while (pageToken != null);
            Console.WriteLine();
        }

        /// <summary>This example adds a new ad unit to a publisher ad client.</summary>
        /// <param name="accountId">The ID for the publisher account to be used.</param>
        /// <param name="adClientId">An arbitrary publisher ad client ID.</param>
        /// <returns>The created ad unit.</returns>
        private AdUnit AddAdUnit(string accountId, string adClientId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Adding ad unit to ad client {0}", accountId);
            Console.WriteLine("=================================================================");

            AdUnit newAdUnit = new AdUnit();

            Random random = new Random(DateTime.Now.Millisecond);
            newAdUnit.Name = "Ad Unit #"
                + random.Next(0, 10000).ToString();

            newAdUnit.ContentAdsSettings = new AdUnit.ContentAdsSettingsData();
            newAdUnit.ContentAdsSettings.BackupOption = new AdUnit.ContentAdsSettingsData.BackupOptionData();

            newAdUnit.ContentAdsSettings.BackupOption.Type = "COLOR";
            newAdUnit.ContentAdsSettings.BackupOption.Color = "ffffff";
            newAdUnit.ContentAdsSettings.Size = "SIZE_200_200";
            newAdUnit.ContentAdsSettings.Type = "TEXT";
            newAdUnit.CustomStyle = new AdStyle();
            newAdUnit.CustomStyle.Colors = new AdStyle.ColorsData();
            newAdUnit.CustomStyle.Colors.Background = "ffffff";
            newAdUnit.CustomStyle.Colors.Border = "000000";
            newAdUnit.CustomStyle.Colors.Text = "000000";
            newAdUnit.CustomStyle.Colors.Title = "000000";
            newAdUnit.CustomStyle.Colors.Url = "0000ff";
            newAdUnit.CustomStyle.Corners = "SQUARE";
            newAdUnit.CustomStyle.Font = new AdStyle.FontData();
            newAdUnit.CustomStyle.Font.Family = "ACCOUNT_DEFAULT_FAMILY";
            newAdUnit.CustomStyle.Font.Size = "ACCOUNT_DEFAULT_SIZE";

            // Create ad unit.
            AccountsResource.AdunitsResource.InsertRequest insertRequest = this.service.Accounts.Adunits
                .Insert(newAdUnit, accountId, adClientId);

            AdUnit adUnit = insertRequest.Execute();

            Console.WriteLine("Ad unit of type {0}, name {1} and status {2} was created",
                adUnit.ContentAdsSettings.Type, adUnit.Name, adUnit.Status);

            Console.WriteLine();

            // Return the Ad Unit that was just created
            return adUnit;
        }

        /// <summary>This example updates an ad unit on a publisher ad client.</summary>
        /// <param name="accountId">The ID for the publisher account to be used.</param>
        /// <param name="adClientId">An arbitrary publisher ad client ID.</param>
        /// <param name="adUnitId">The ID of the ad unit to be updated.</param>
        /// <returns>The updated custom channel.</returns>
        private AdUnit UpdateAdUnit(string accountId, string adClientId, string adUnitId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Updating ad unit {0}", adUnitId);
            Console.WriteLine("=================================================================");

            AdUnit patchAdUnit = new AdUnit();
            patchAdUnit.CustomStyle = new AdStyle();
            patchAdUnit.CustomStyle.Colors = new AdStyle.ColorsData();
            patchAdUnit.CustomStyle.Colors.Text = "ff0000";

            // Update custom channel: Using REST's PATCH method to update just the Name field.
            AdUnit adUnit = this.service.Accounts.Adunits
                .Patch(patchAdUnit, accountId, adClientId, adUnitId).Execute();

            Console.WriteLine("Ad unit with id {0}, was updated with text color {1}.",
                adUnit.Id, adUnit.CustomStyle.Colors.Text);

            Console.WriteLine();

            // Return the Ad Unit that was just created
            return adUnit;
        }

        /// <summary>This example deletes an Ad Unit on a publisher ad client.</summary>
        /// <param name="accountId">The ID for the publisher account to be used.</param>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="adUnitId">The ID for the Ad Unit to be deleted.</param>
        private void DeleteAdUnit(string accountId, string adClientId, string adUnitId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Deleting ad unit {0}", adUnitId);
            Console.WriteLine("=================================================================");

            // Delete ad unit
            AdUnit adUnit = this.service.Accounts.Adunits.Delete(accountId, adClientId, adUnitId).Execute();

            Console.WriteLine("Ad unit with id {0} was deleted.", adUnitId);

            Console.WriteLine();
        }

        /// <summary>
        /// This example retrieves a report for the specified publisher ad client.
        /// 
        /// Note that the statistics returned in these reports only include data from ad
        /// units created with the AdSense Host API v4.x.
        /// </summary>
        /// <param name="accountId">The ID of the publisher account on which to run the report.</param>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        private void GenerateReport(string accountId, string adClientId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Running report for ad client {0}", adClientId);
            Console.WriteLine("=================================================================");

            // Prepare report.
            var startDate = DateTime.Today.ToString(DateFormat);
            var endDate = DateTime.Today.AddDays(-7).ToString(DateFormat);
            AccountsResource.ReportsResource.GenerateRequest reportRequest
                = this.service.Accounts.Reports.Generate(accountId, startDate, endDate);

            // Specify the desired ad client using a filter, as well as other parameters.
            // A complete list of metrics and dimensions is available on the documentation.

            reportRequest.Filter = new List<string> { "AD_CLIENT_ID==" 
                + ReportHelper.EscapeFilterParameter(adClientId) };
            reportRequest.Metric = new List<string> { "PAGE_VIEWS", "AD_REQUESTS", "AD_REQUESTS_COVERAGE", 
                "CLICKS", "AD_REQUESTS_CTR", "COST_PER_CLICK", "AD_REQUESTS_RPM", "EARNINGS" };
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
                Console.WriteLine("No rows returned.");
            }

            Console.WriteLine();
        }
    }
}
