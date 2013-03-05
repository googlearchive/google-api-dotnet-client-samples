/*
Copyright 2013 Google Inc

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

using Google.Apis.Adsense.v1_2;
using Google.Apis.Adsense.v1_2.Data;
using Google.Apis.Samples.Helper;
using Google.Apis.Util;

namespace AdSense.Sample
{
    /// <summary>
    /// A sample consumer that runs multiple requests against the AdSense Management API.
    /// These include:
    /// <list type="bullet">
    /// <item>
    /// <description>Retrieves the list of accounts</description> 
    /// </item>
    /// <item>
    /// <description>Retrieves the list of ad clients</description> 
    /// </item>
    /// <item>
    /// <description>Retrieves the list of ad units for a random ad client</description> 
    /// </item>
    /// <item>
    /// <description>Retrieves the list of custom channels for a random ad unit</description> 
    /// </item>
    /// <item>
    /// <description>Retrieves the list of custom channels</description> 
    /// </item>
    /// <item>
    /// <description>Retrieves the list of ad units tagged by a random custom channel</description> 
    /// </item>
    /// <item>
    /// <description>Retrieves the list of URL channels for the logged in user</description> 
    /// </item>
    /// <item>
    /// <description>Retrieves the list of saved ad styles for the logged in user</description> 
    /// </item>
    /// <item>
    /// <description>Retrieves the list of saved reports for the logged in user</description> 
    /// </item>
    /// <item>
    /// <description>Generates a random saved report</description> 
    /// </item>
    /// <item>
    /// <description>Generates a saved report</description> 
    /// </item>
    /// <item>
    /// <description>Generates a saved report with paging</description> 
    /// </item>
    /// </list> 
    /// </summary>
    public class ManagementApiConsumer
    {
        private static readonly string DateFormat = "yyyy-MM-dd";
        private AdsenseService service;
        private int maxListPageSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementApiConsumer"/> class.
        /// </summary>
        /// <param name="service">AdSense service object on which to run the requests.</param>
        /// <param name="maxListPageSize">The maximum page size to retrieve.</param>
        public ManagementApiConsumer(AdsenseService service, int maxListPageSize)
        {
            this.service = service;
            this.maxListPageSize = maxListPageSize;
        }

        /// <summary>
        /// Runs multiple Publisher requests against the AdSense Management API.
        /// </summary>
        internal void RunCalls()
        {
            Accounts accounts = GetAllAccounts();

            // Get an example account, so we can run the following samples.
            var exampleAccount = accounts.Items.NullToEmpty().FirstOrDefault();
            if (exampleAccount != null)
            {
                DisplayAccountTree(exampleAccount.Id);
                DisplayAllAdClientsForAccount(exampleAccount.Id);
            }

            var adClients = GetAllAdClients();

            // Get an ad client, so we can run the rest of the samples.
            var exampleAdClient = adClients.Items.NullToEmpty().FirstOrDefault();
            if (exampleAdClient != null)
            {
                var adUnits = GetAllAdUnits(exampleAdClient.Id);

                // Get an example ad unit, so we can run the following sample.
                var exampleAdUnit = adUnits.Items.NullToEmpty().FirstOrDefault();
                if (exampleAdUnit != null)
                {
                    DisplayAllCustomChannelsForAdUnit(exampleAdClient.Id, exampleAdUnit.Id);
                }

                var customChannels = GetAllCustomChannels(exampleAdClient.Id);

                // Get an example custom channel, so we can run the following sample.
                var exampleCustomChannel = customChannels.Items.NullToEmpty().FirstOrDefault();
                if (exampleCustomChannel != null)
                {
                    DisplayAllAdUnits(exampleAdClient.Id, exampleCustomChannel.Id);
                }

                DisplayAllUrlChannels(exampleAdClient.Id);
                DisplayAllSavedAdStyles();

                SavedReports savedReports = GetAllSavedReports();

                // Get an example saved report, so we can run the following sample.
                var exampleSavedReport = savedReports.Items.NullToEmpty().FirstOrDefault();
                if (exampleSavedReport != null)
                {
                    GenerateSavedReport(exampleSavedReport.Id);
                }

                GenerateReport(exampleAdClient.Id);
                GenerateReportWithPaging(exampleAdClient.Id);
            }

            CommandLine.PressAnyKeyToExit();
        }

        /// <summary>
        /// Gets and prints all accounts for the logged in user.
        /// </summary>
        /// <returns>The last page of retrieved accounts.</returns>
        private Accounts GetAllAccounts()
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all AdSense accounts");
            CommandLine.WriteLine("=================================================================");

            // Retrieve account list in pages and display data as we receive it.
            string pageToken = null;
            Accounts accountResponse = null;

            do
            {
                var accountRequest = this.service.Accounts.List();
                accountRequest.MaxResults = this.maxListPageSize;
                accountRequest.PageToken = pageToken;
                accountResponse = accountRequest.Fetch();

                if (accountResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var account in accountResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Account with ID \"{0}\" and name \"{1}\" was found.",
                            account.Id,
                            account.Name);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No accounts found.");
                }

                pageToken = accountResponse.NextPageToken;
            } 
            while (pageToken != null);

            CommandLine.WriteLine();

            // Return the last page of accounts, so that the main sample has something to run.
            return accountResponse;
        }

        /// <summary>
        /// Displays the AdSense account tree for a given account.
        /// </summary>
        /// <param name="accountId">The ID for the account to be used.</param>
        private void DisplayAccountTree(string accountId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Displaying AdSense account tree for {0}", accountId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve account.
            var account = this.service.Accounts.Get(accountId).Fetch();
            this.DisplayTree(account, 0);

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Auxiliary method to recurse through the account tree, displaying it.
        /// </summary>
        /// <param name="parentAccount">The account to print a sub-tree for.</param>
        /// <param name="level">The depth at which the top account exists in the tree.</param>
        private void DisplayTree(Account parentAccount, int level)
        {
            CommandLine.WriteLine(
                "{0}Account with ID \"{1}\" and name \"{2}\" was found.",
                new string(' ', 2 * level), 
                parentAccount.Id, 
                parentAccount.Name);

            foreach (var subAccount in parentAccount.SubAccounts.NullToEmpty())
            {
                DisplayTree(subAccount, level + 1);
            }
        }

        /// <summary>
        /// Displays all ad clients for an account.
        /// </summary>
        /// <param name="accountId">The ID for the account to be used.</param>
        private void DisplayAllAdClientsForAccount(string accountId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all ad clients for account {0}", accountId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdClients adClientResponse = null;

            do
            {
                var adClientRequest = this.service.Accounts.Adclients.List(accountId);
                adClientRequest.MaxResults = this.maxListPageSize;
                adClientRequest.PageToken = pageToken;
                adClientResponse = adClientRequest.Fetch();

                if (adClientResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var adClient in adClientResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Ad client for product \"{0}\" with ID \"{1}\" was found.",
                            adClient.ProductCode, 
                            adClient.Id);
                        CommandLine.WriteLine(
                            "\tSupports reporting: {0}",
                            adClient.SupportsReporting.Value ? "Yes" : "No");
                    }
                }
                else
                {
                    CommandLine.WriteLine("No ad clients found.");
                }

                pageToken = adClientResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Gets and prints all ad clients for the logged in user's default account.
        /// </summary>
        /// <returns>The last page of retrieved accounts.</returns>
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
                adClientResponse = adClientRequest.Fetch();

                if (adClientResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var adClient in adClientResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Ad client for product \"{0}\" with ID \"{1}\" was found.",
                            adClient.ProductCode, 
                            adClient.Id);
                        CommandLine.WriteLine(
                            "\tSupports reporting: {0}",
                            adClient.SupportsReporting.Value ? "Yes" : "No");
                    }
                }
                else
                {
                    CommandLine.WriteLine("No ad clients found.");
                }

                pageToken = adClientResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();

            // Return the last page of ad clients, so that the main sample has something to run.
            return adClientResponse;
        }

        /// <summary>
        /// Gets and prints all ad units in an ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <returns>The last page of retrieved accounts.</returns>
        private AdUnits GetAllAdUnits(string adClientId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all ad units for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdUnits adUnitResponse = null;

            do
            {
                var adUnitRequest = this.service.Adunits.List(adClientId);
                adUnitRequest.MaxResults = this.maxListPageSize;
                adUnitRequest.PageToken = pageToken;
                adUnitResponse = adUnitRequest.Fetch();

                if (adUnitResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var adUnit in adUnitResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Ad unit with code \"{0}\", name \"{1}\" and status \"{2}\" " +
                               "was found.", 
                            adUnit.Code, 
                            adUnit.Name, 
                            adUnit.Status);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No ad units found.");
                }

                pageToken = adUnitResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();

            // Return the last page of ad units, so that the main sample has something to run.
            return adUnitResponse;
        }

        /// <summary>
        /// Gets and prints all custom channels in an ad client.
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
                customChannelResponse = customChannelRequest.Fetch();

                if (customChannelResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var customChannel in customChannelResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Custom channel with code \"{0}\" and name \"{1}\" was found.",
                            customChannel.Code, 
                            customChannel.Name);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No custom channels found.");
                }

                pageToken = customChannelResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();

            // Return the last page of custom channels, so that the main sample has something to run.
            return customChannelResponse;
        }

        /// <summary>
        /// Prints all ad units corresponding to a specified custom channel.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="customChannelId">The ID for the custom channel to be used.</param>
        private void DisplayAllAdUnits(string adClientId, string customChannelId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all ad units for custom channel {0}", customChannelId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdUnits adUnitResponse = null;

            do
            {
                var adUnitRequest = this.service.Customchannels.Adunits.List(adClientId, customChannelId);
                adUnitRequest.MaxResults = this.maxListPageSize;
                adUnitRequest.PageToken = pageToken;
                adUnitResponse = adUnitRequest.Fetch();

                if (adUnitResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var adUnit in adUnitResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Ad unit with code \"{0}\", name \"{1}\" and status \"{2}\" " +
                                "was found.",
                            adUnit.Code,
                            adUnit.Name,
                            adUnit.Status);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No ad units found.");
                }

                pageToken = adUnitResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Displays all custom channels an ad unit has been added to.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="adUnitId">The ID for the ad unit to be used.</param>
        private void DisplayAllCustomChannelsForAdUnit(string adClientId, string adUnitId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all custom channels for ad unit {0}", adUnitId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve custom channel list in pages and display data as we receive it.
            string pageToken = null;
            CustomChannels customChannelResponse = null;

            do
            {
                var customChannelRequest = this.service.Adunits.Customchannels.List(adClientId, adUnitId);
                customChannelRequest.MaxResults = this.maxListPageSize;
                customChannelRequest.PageToken = pageToken;
                customChannelResponse = customChannelRequest.Fetch();

                if (customChannelResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var customChannel in customChannelResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Custom channel with code \"{0}\" and name \"{1}\" was found.",
                            customChannel.Code, 
                            customChannel.Name);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No custom channels found.");
                }

                pageToken = customChannelResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Displays all URL channels in an ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        private void DisplayAllUrlChannels(string adClientId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all URL channels for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve URL channel list in pages and display data as we receive it.
            string pageToken = null;
            UrlChannels urlChannelResponse = null;

            do
            {
                var urlChannelRequest = this.service.Urlchannels.List(adClientId);
                urlChannelRequest.MaxResults = this.maxListPageSize;
                urlChannelRequest.PageToken = pageToken;
                urlChannelResponse = urlChannelRequest.Fetch();

                if (urlChannelResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var urlChannel in urlChannelResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "URL channel with pattern \"{0}\" was found.",
                            urlChannel.UrlPattern);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No URL channels found.");
                }

                pageToken = urlChannelResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Retrieves a report, using a filter for a specified ad client.
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        private void GenerateReport(string adClientId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Running report for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Prepare report.
            var startDate = DateTime.Today.ToString(DateFormat);
            var endDate = DateTime.Today.AddDays(-7).ToString(DateFormat);
            var reportRequest = this.service.Reports.Generate(startDate, endDate);

            // Specify the desired ad client using a filter, as well as other parameters.
            reportRequest.Filter = new List<string> 
            {
                "AD_CLIENT_ID==" + 
                ReportUtils.EscapeFilterParameter(adClientId) 
            };
            reportRequest.Metric = new List<string> 
            {
                "PAGE_VIEWS", "AD_REQUESTS", "AD_REQUESTS_COVERAGE",
                "AD_REQUESTS_CTR", "COST_PER_CLICK", "AD_REQUESTS_RPM", "EARNINGS" 
            };
            reportRequest.Dimension = new List<string> { "DATE" };
            reportRequest.Sort = new List<string> { "+DATE" };

            // Run report.
            var reportResponse = reportRequest.Fetch();

            if (reportResponse.Rows.IsNotNullOrEmpty())
            {
                ReportUtils.DisplayHeaders(reportResponse.Headers);
                ReportUtils.DisplayRows(reportResponse.Rows);
            }
            else
            {
                CommandLine.WriteLine("No rows returned.");
            }

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Retrieves a report for a specified ad client, using pagination.
        /// <para>Please only use pagination if your application requires it due to memory or storage constraints.
        /// If you need to retrieve more than 5000 rows, please check GenerateReport, as due to current
        /// limitations you will not be able to use paging for large reports.</para>
        /// </summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        private void GenerateReportWithPaging(string adClientId)
        {
            int rowLimit = 5000;

            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Running paginated report for ad client {0}", adClientId);
            CommandLine.WriteLine("=================================================================");

            // Prepare report.
            var startDate = DateTime.Today.ToString(DateFormat);
            var endDate = DateTime.Today.AddDays(-7).ToString(DateFormat);
            var reportRequest = this.service.Reports.Generate(startDate, endDate);
            var pageSize = this.maxListPageSize;
            var startIndex = 0;

            // Specify the desired ad client using a filter, as well as other parameters.
            reportRequest.Filter = new List<string> 
            {
                "AD_CLIENT_ID==" + 
                ReportUtils.EscapeFilterParameter(adClientId) 
            };
            reportRequest.Metric = new List<string> 
            {
                "PAGE_VIEWS", "AD_REQUESTS", "AD_REQUESTS_COVERAGE",
                "AD_REQUESTS_CTR", "COST_PER_CLICK", "AD_REQUESTS_RPM", "EARNINGS" 
            };
            reportRequest.Dimension = new List<string> { "DATE" };
            reportRequest.Sort = new List<string> { "+DATE" };

            // Run first page of report.
            var reportResponse = ReportUtils.GetPage(reportRequest, startIndex, pageSize);

            if (reportResponse.Rows.IsNullOrEmpty())
            {
                CommandLine.WriteLine("No rows returned.");
                return;
            }

            // Display headers.
            ReportUtils.DisplayHeaders(reportResponse.Headers);

            // Display first page of results.
            ReportUtils.DisplayRows(reportResponse.Rows);

            var totalRows = Math.Min(int.Parse(reportResponse.TotalMatchedRows), rowLimit);
            for (startIndex = reportResponse.Rows.Count; startIndex < totalRows;
                startIndex += reportResponse.Rows.Count)
            {
                // Check to see if we're going to go above the limit and get as many results as we can.
                pageSize = Math.Min(this.maxListPageSize, totalRows - startIndex);

                // Run next page of report.
                reportResponse = ReportUtils.GetPage(reportRequest, startIndex, pageSize);

                // If the report size changes in between paged requests, the result may be empty.
                if (reportResponse.Rows.IsNullOrEmpty())
                {
                    break;
                }

                // Display results.
                ReportUtils.DisplayRows(reportResponse.Rows);
            }

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Retrieves a report, using a filter for a specified saved report.
        /// </summary>
        /// <param name="savedReportId">The ID of the saved report to generate.</param>
        private void GenerateSavedReport(string savedReportId)
        {
            ReportsResource.SavedResource.GenerateRequest savedReportRequest = this.service.Reports.Saved.Generate(savedReportId);
            AdsenseReportsGenerateResponse savedReportResponse = savedReportRequest.Fetch();

            // Run report.
            if (savedReportResponse.Rows.IsNotNullOrEmpty())
            {
                ReportUtils.DisplayHeaders(savedReportResponse.Headers);
                ReportUtils.DisplayRows(savedReportResponse.Rows);
            }
            else
            {
                CommandLine.WriteLine("No rows returned.");
            }

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Gets and prints all the saved reports for the logged in user's default account.
        /// </summary>
        /// <returns>The last page of the retrieved saved reports.</returns>
        private SavedReports GetAllSavedReports()
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all saved reports");
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            SavedReports savedReportResponse = null;

            do
            {
                var savedReportRequest = this.service.Reports.Saved.List();
                savedReportRequest.MaxResults = this.maxListPageSize;
                savedReportRequest.PageToken = pageToken;
                savedReportResponse = savedReportRequest.Fetch();

                if (savedReportResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var savedReport in savedReportResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Saved report with ID \"{0}\" and name \"{1}\" was found.",
                            savedReport.Id,
                            savedReport.Name);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No saved saved reports found.");
                }

                pageToken = savedReportResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();
            return savedReportResponse;
        }

        /// <summary>
        /// Displays all the saved ad styles for the logged in user's default account.
        /// </summary>
        private void DisplayAllSavedAdStyles()
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all saved ad styles");
            CommandLine.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            SavedAdStyles savedAdStyleResponse = null;

            do
            {
                var savedAdStyleRequest = this.service.Savedadstyles.List();
                savedAdStyleRequest.MaxResults = this.maxListPageSize;
                savedAdStyleRequest.PageToken = pageToken;
                savedAdStyleResponse = savedAdStyleRequest.Fetch();

                if (savedAdStyleResponse.Items.IsNotNullOrEmpty())
                {
                    foreach (var savedAdStyle in savedAdStyleResponse.Items)
                    {
                        CommandLine.WriteLine(
                            "Saved ad style with ID \"{0}\" and background color \"{1}\" was found.",
                            savedAdStyle.Id,
                            savedAdStyle.AdStyle.Colors.Background);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No saved ad styles found.");
                }

                pageToken = savedAdStyleResponse.NextPageToken;
            }
            while (pageToken != null);

            CommandLine.WriteLine();
        }
    }
}
