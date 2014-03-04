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

using Google.Apis.AdSense.v1_4;
using Google.Apis.AdSense.v1_4.Data;

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
        private AdSenseService service;
        private int maxListPageSize;
        private Account adSenseAccount;

        /// <summary>Initializes a new instance of the <see cref="ManagementApiConsumer"/> class.</summary>
        /// <param name="service">AdSense service object on which to run the requests.</param>
        /// <param name="maxListPageSize">The maximum page size to retrieve.</param>
        public ManagementApiConsumer(AdSenseService service, int maxListPageSize)
        {
            this.service = service;
            this.maxListPageSize = maxListPageSize;
        }

        /// <summary>Runs multiple Publisher requests against the AdSense Management API.</summary>
        internal void RunCalls()
        {
            Accounts accounts = GetAllAccounts();

            // Get an example account, so we can run the following samples.
            adSenseAccount = accounts.Items.NullToEmpty().FirstOrDefault();
            if (adSenseAccount != null)
            {
                DisplayAccountTree(adSenseAccount.Id);
                DisplayAllAdClientsForAccount(adSenseAccount.Id);
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

            DisplayAllMetricsAndDimensions();
            DisplayAllAlerts();
        }

        /// <summary>Gets and prints all accounts for the logged in user.</summary>
        /// <returns>The last page of retrieved accounts.</returns>
        private Accounts GetAllAccounts()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all AdSense accounts");
            Console.WriteLine("=================================================================");

            // Retrieve account list in pages and display data as we receive it.
            string pageToken = null;
            Accounts accountResponse = null;

            do
            {
                var accountRequest = service.Accounts.List();
                accountRequest.MaxResults = maxListPageSize;
                accountRequest.PageToken = pageToken;
                accountResponse = accountRequest.Execute();

                if (!accountResponse.Items.IsNullOrEmpty())
                {
                    foreach (var account in accountResponse.Items)
                    {
                        Console.WriteLine(
                            "Account with ID \"{0}\" and name \"{1}\" was found.", 
                            account.Id,
                            account.Name);
                    }
                }
                else
                {
                    Console.WriteLine("No accounts found.");
                }

                pageToken = accountResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();

            // Return the last page of accounts, so that the main sample has something to run.
            return accountResponse;
        }

        /// <summary>Displays the AdSense account tree for a given account.</summary>
        /// <param name="accountId">The ID for the account to be used.</param>
        private void DisplayAccountTree(string accountId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Displaying AdSense account tree for {0}", accountId);
            Console.WriteLine("=================================================================");

            // Retrieve account.
            var account = service.Accounts.Get(accountId).Execute();
            DisplayTree(account, 0);

            Console.WriteLine();
        }

        /// <summary>
        /// Auxiliary method to recurse through the account tree, displaying it.
        /// </summary>
        /// <param name="parentAccount">The account to print a sub-tree for.</param>
        /// <param name="level">The depth at which the top account exists in the tree.</param>
        private void DisplayTree(Account parentAccount, int level)
        {
            Console.WriteLine(
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
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all ad clients for account {0}", accountId);
            Console.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdClients adClientResponse = null;

            do
            {
                var adClientRequest = service.Accounts.Adclients.List(accountId);
                adClientRequest.MaxResults = maxListPageSize;
                adClientRequest.PageToken = pageToken;
                adClientResponse = adClientRequest.Execute();

                if (!adClientResponse.Items.IsNullOrEmpty())
                {
                    foreach (var adClient in adClientResponse.Items)
                    {
                        Console.WriteLine(
                            "Ad client for product \"{0}\" with ID \"{1}\" was found.",
                            adClient.ProductCode, 
                           adClient.Id);
                        Console.WriteLine(
                            "\tSupports reporting: {0}",
                            adClient.SupportsReporting.Value ? "Yes" : "No");
                    }
                }
                else
                {
                    Console.WriteLine("No ad clients found.");
                }

                pageToken = adClientResponse.NextPageToken;
            }
            while (pageToken != null);

            Console.WriteLine();
        }

        /// <summary>
        /// Gets and prints all ad clients for the logged in user's default account.
        /// </summary>
        /// <returns>The last page of retrieved accounts.</returns>
        private AdClients GetAllAdClients()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all ad clients for default account");
            Console.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdClients adClientResponse = null;

            do
            {
                var adClientRequest = service.Accounts.Adclients.List(adSenseAccount.Id);
                adClientRequest.MaxResults = maxListPageSize;
                adClientRequest.PageToken = pageToken;
                adClientResponse = adClientRequest.Execute();

                if (!adClientResponse.Items.IsNullOrEmpty())
                {
                    foreach (var adClient in adClientResponse.Items)
                    {
                        Console.WriteLine(
                            "Ad client for product \"{0}\" with ID \"{1}\" was found.",
                            adClient.ProductCode,
                            adClient.Id);
                        Console.WriteLine(
                            "\tSupports reporting: {0}",
                            adClient.SupportsReporting.Value ? "Yes" : "No");
                    }
                }
                else
                {
                    Console.WriteLine("No ad clients found.");
                }

                pageToken = adClientResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();

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
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all ad units for ad client {0}", adClientId);
            Console.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdUnits adUnitResponse = null;

            do
            {
                var adUnitRequest = service.Adunits.List(adClientId);
                adUnitRequest.MaxResults = maxListPageSize;
                adUnitRequest.PageToken = pageToken;
                adUnitResponse = adUnitRequest.Execute();

                if (!adUnitResponse.Items.IsNullOrEmpty())
                {
                    foreach (var adUnit in adUnitResponse.Items)
                    {
                        Console.WriteLine(
                            "Ad unit with code \"{0}\", name \"{1}\" and status \"{2}\" was found.",
                            adUnit.Code,
                            adUnit.Name,
                            adUnit.Status);
                    }
                }
                else
                {
                    Console.WriteLine("No ad units found.");
                }

                pageToken = adUnitResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();

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
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all custom channels for ad client {0}", adClientId);
            Console.WriteLine("=================================================================");

            // Retrieve custom channel list in pages and display data as we receive it.
            string pageToken = null;
            CustomChannels customChannelResponse = null;

            do
            {
                var customChannelRequest = service.Accounts.Customchannels.List(adSenseAccount.Id, adClientId);
                customChannelRequest.MaxResults = maxListPageSize;
                customChannelRequest.PageToken = pageToken;
                customChannelResponse = customChannelRequest.Execute();

                if (!customChannelResponse.Items.IsNullOrEmpty())
                {
                    foreach (var customChannel in customChannelResponse.Items)
                    {
                        Console.WriteLine(
                            "Custom channel with code \"{0}\" and name \"{1}\" was found.",
                            customChannel.Code,
                            customChannel.Name);
                    }
                }
                else
                {
                    Console.WriteLine("No custom channels found.");
                }

                pageToken = customChannelResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();

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
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all ad units for custom channel {0}", customChannelId);
            Console.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            AdUnits adUnitResponse = null;

            do
            {
                var adUnitRequest = service.Accounts.Customchannels.Adunits.List(
                    adSenseAccount.Id,
                    adClientId,
                    customChannelId);
                adUnitRequest.MaxResults = maxListPageSize;
                adUnitRequest.PageToken = pageToken;
                adUnitResponse = adUnitRequest.Execute();

                if (!adUnitResponse.Items.IsNullOrEmpty())
                {
                    foreach (var adUnit in adUnitResponse.Items)
                    {
                        Console.WriteLine(
                            "Ad unit with code \"{0}\", name \"{1}\" and status \"{2}\" was found.",
                            adUnit.Code,
                            adUnit.Name,
                            adUnit.Status);
                    }
                }
                else
                {
                    Console.WriteLine("No ad units found.");
                }

                pageToken = adUnitResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();
        }

        /// <summary>Displays all custom channels an ad unit has been added to.</summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        /// <param name="adUnitId">The ID for the ad unit to be used.</param>
        private void DisplayAllCustomChannelsForAdUnit(string adClientId, string adUnitId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all custom channels for ad unit {0}", adUnitId);
            Console.WriteLine("=================================================================");

            // Retrieve custom channel list in pages and display data as we receive it.
            string pageToken = null;
            CustomChannels customChannelResponse = null;

            do
            {
                var customChannelRequest = service.Accounts.Adunits.Customchannels.List(
                    adSenseAccount.Id,
                    adClientId,
                    adUnitId);
                customChannelRequest.MaxResults = maxListPageSize;
                customChannelRequest.PageToken = pageToken;
                customChannelResponse = customChannelRequest.Execute();

                if (!customChannelResponse.Items.IsNullOrEmpty())
                {
                    foreach (var customChannel in customChannelResponse.Items)
                    {
                        Console.WriteLine(
                            "Custom channel with code \"{0}\" and name \"{1}\" was found.",
                            customChannel.Code,
                            customChannel.Name);
                    }
                }
                else
                {
                    Console.WriteLine("No custom channels found.");
                }

                pageToken = customChannelResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();
        }

        /// <summary>Displays all URL channels in an ad client.</summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        private void DisplayAllUrlChannels(string adClientId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all URL channels for ad client {0}", adClientId);
            Console.WriteLine("=================================================================");

            // Retrieve URL channel list in pages and display data as we receive it.
            string pageToken = null;
            UrlChannels urlChannelResponse = null;

            do
            {
                var urlChannelRequest = service.Accounts.Urlchannels.List(adSenseAccount.Id, adClientId);
                urlChannelRequest.MaxResults = maxListPageSize;
                urlChannelRequest.PageToken = pageToken;
                urlChannelResponse = urlChannelRequest.Execute();

                if (!urlChannelResponse.Items.IsNullOrEmpty())
                {
                    foreach (var urlChannel in urlChannelResponse.Items)
                    {
                        Console.WriteLine("URL channel with pattern \"{0}\" was found.", urlChannel.UrlPattern);
                    }
                }
                else
                {
                    Console.WriteLine("No URL channels found.");
                }

                pageToken = urlChannelResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();
        }

        /// <summary>Retrieves a report, using a filter for a specified ad client.</summary>
        /// <param name="adClientId">The ID for the ad client to be used.</param>
        private void GenerateReport(string adClientId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Running report for ad client {0}", adClientId);
            Console.WriteLine("=================================================================");

            // Prepare report.
            var startDate = DateTime.Today.AddDays(-7).ToString(DateFormat);
            var endDate = DateTime.Today.ToString(DateFormat);
            var reportRequest = service.Accounts.Reports.Generate(adSenseAccount.Id, startDate, endDate);

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
            var reportResponse = reportRequest.Execute();

            if (!reportResponse.Rows.IsNullOrEmpty())
            {
                ReportUtils.DisplayHeaders(reportResponse.Headers);
                Console.WriteLine("Showing data from {0} to {1}", reportResponse.StartDate, reportResponse.EndDate);
                ReportUtils.DisplayRows(reportResponse.Rows);
            }
            else
            {
                Console.WriteLine("No rows returned.");
            }

            Console.WriteLine();
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

            Console.WriteLine("=================================================================");
            Console.WriteLine("Running paginated report for ad client {0}", adClientId);
            Console.WriteLine("=================================================================");

            // Prepare report.
            var startDate = DateTime.Today.AddDays(-7).ToString(DateFormat);
            var endDate = DateTime.Today.ToString(DateFormat);
            var reportRequest = service.Accounts.Reports.Generate(adSenseAccount.Id, startDate, endDate);
            var pageSize = maxListPageSize;
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
                Console.WriteLine("No rows returned.");
                Console.WriteLine();
                return;
            }

            // Display headers.
            ReportUtils.DisplayHeaders(reportResponse.Headers);

            // Display first page of results.
            ReportUtils.DisplayRows(reportResponse.Rows);

            var totalRows = Math.Min(reportResponse.TotalMatchedRows.Value, rowLimit);
            for (startIndex = reportResponse.Rows.Count; startIndex < totalRows;
                startIndex += reportResponse.Rows.Count)
            {
                // Check to see if we're going to go above the limit and get as many results as we can.
                pageSize = Math.Min(maxListPageSize, (int)totalRows - startIndex);

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

            Console.WriteLine();
        }

        /// <summary>
        /// Retrieves a report, using a filter for a specified saved report.
        /// </summary>
        /// <param name="savedReportId">The ID of the saved report to generate.</param>
        private void GenerateSavedReport(string savedReportId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Running saved report {0}", savedReportId);
            Console.WriteLine("=================================================================");

            var savedReportRequest = service.Accounts.Reports.Saved.Generate(adSenseAccount.Id, savedReportId);
            AdsenseReportsGenerateResponse savedReportResponse = savedReportRequest.Execute();

            // Run report.
            if (!savedReportResponse.Rows.IsNullOrEmpty())
            {
                ReportUtils.DisplayHeaders(savedReportResponse.Headers);
                ReportUtils.DisplayRows(savedReportResponse.Rows);
            }
            else
            {
                Console.WriteLine("No rows returned.");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Gets and prints all the saved reports for the logged in user's default account.
        /// </summary>
        /// <returns>The last page of the retrieved saved reports.</returns>
        private SavedReports GetAllSavedReports()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all saved reports");
            Console.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            SavedReports savedReportResponse = null;

            do
            {
                var savedReportRequest = service.Accounts.Reports.Saved.List(adSenseAccount.Id);
                savedReportRequest.MaxResults = maxListPageSize;
                savedReportRequest.PageToken = pageToken;
                savedReportResponse = savedReportRequest.Execute();

                if (!savedReportResponse.Items.IsNullOrEmpty())
                {
                    foreach (var savedReport in savedReportResponse.Items)
                    {
                        Console.WriteLine(
                            "Saved report with ID \"{0}\" and name \"{1}\" was found.",
                            savedReport.Id,
                            savedReport.Name);
                    }
                }
                else
                {
                    Console.WriteLine("No saved saved reports found.");
                }

                pageToken = savedReportResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();
            return savedReportResponse;
        }

        /// <summary>Displays all the saved ad styles for the logged in user's default account.</summary>
        private void DisplayAllSavedAdStyles()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all saved ad styles");
            Console.WriteLine("=================================================================");

            // Retrieve ad client list in pages and display data as we receive it.
            string pageToken = null;
            SavedAdStyles savedAdStyleResponse = null;

            do
            {
                var savedAdStyleRequest = service.Savedadstyles.List();
                savedAdStyleRequest.MaxResults = maxListPageSize;
                savedAdStyleRequest.PageToken = pageToken;
                savedAdStyleResponse = savedAdStyleRequest.Execute();

                if (!savedAdStyleResponse.Items.IsNullOrEmpty())
                {
                    foreach (var savedAdStyle in savedAdStyleResponse.Items)
                    {
                        Console.WriteLine(
                            "Saved ad style with ID \"{0}\" and name \"{1}\" was found.",
                            savedAdStyle.Id,
                            savedAdStyle.Name);
                    }
                }
                else
                {
                    Console.WriteLine("No saved ad styles found.");
                }

                pageToken = savedAdStyleResponse.NextPageToken;
            }
            while (pageToken != null);
            Console.WriteLine();
        }

        /// <summary>
        /// Displays all the available metrics and dimensions for the logged in user's default account.
        /// </summary>
        private void DisplayAllMetricsAndDimensions()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all metrics");
            Console.WriteLine("=================================================================");

            Metadata metricsResponse = service.Metadata.Metrics.List().Execute();

            if (!metricsResponse.Items.IsNullOrEmpty())
            {
                foreach (var metric in metricsResponse.Items)
                {
                    Console.WriteLine(
                        "Metric with ID \"{0}\" is available for products: \"{1}\".",
                        metric.Id,
                        String.Join(", ", metric.SupportedProducts.ToArray()));
                }
            }
            else
            {
                Console.WriteLine("No available metrics found.");
            }

            Console.WriteLine();

            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all dimensions");
            Console.WriteLine("=================================================================");

            Metadata dimensionsResponse = service.Metadata.Dimensions.List().Execute();

            if (!dimensionsResponse.Items.IsNullOrEmpty())
            {
                foreach (var dimension in dimensionsResponse.Items)
                {
                    if (dimension.SupportedProducts.IsNullOrEmpty())
                    {
                        Console.WriteLine("Dimension with ID \"{0}\" is available for no products.", dimension.Id);
                    }
                    else
                    {
                        Console.WriteLine(
                            "Dimension with ID \"{0}\" is available for products: \"{1}\".",
                            dimension.Id,
                            String.Join(", ", dimension.SupportedProducts.ToArray()));
                    }
                }
            }
            else
            {
                Console.WriteLine("No available dimensions found.");
            }

            Console.WriteLine();
        }

        /// <summary>Prints all the alerts for the logged in user's default account.</summary>
        private void DisplayAllAlerts()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all alerts");
            Console.WriteLine("=================================================================");

            Alerts alertsResponse = service.Alerts.List().Execute();

            if (!alertsResponse.Items.IsNullOrEmpty())
            {
                foreach (var alert in alertsResponse.Items)
                {
                    Console.WriteLine(
                        "Alert with ID \"{0}\" type \"{1}\" and severity \"{2}\" was found.",
                        alert.Id,
                        alert.Type,
                        alert.Severity);

                    // Uncomment to dismiss (delete) the alert. Note that there is no way to revert this.
                    // service.Alerts.Delete(alert.Id);
                }
            }
            else
            {
                Console.WriteLine("No alerts found.");
            }

            Console.WriteLine();
        }

        /// <summary>Prints all the alerts for the logged in user's default account.</summary>
        private void DisplayAllPayments()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing all payments");
            Console.WriteLine("=================================================================");

            Payments paymentsResponse = service.Payments.List().Execute();

            if (!paymentsResponse.Items.IsNullOrEmpty())
            {
                foreach (var payment in paymentsResponse.Items)
                {
                    Console.WriteLine(
                        "Payment with ID \"{0}\" of {1}{2} and date \"{3}\" was found.",
                        payment.Id,
                        payment.PaymentAmount,
                        payment.PaymentAmountCurrencyCode,
                        payment.PaymentDate);
                }
            }
            else
            {
                Console.WriteLine("No payments found.");
            }

            Console.WriteLine();
        }
    }
}
