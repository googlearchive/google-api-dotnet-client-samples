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
    /// This example gets all accounts for the logged in user.
    ///
    /// Tags: accounts.list
    /// </summary>
    class GetAllAccounts
    {
        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="maxPageSize">The maximum page size to retrieve.</param>
        /// <returns>The last page of retrieved ad clients.</returns>
        public static Accounts Run(AdsenseService adsense, int maxPageSize)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all AdSense accounts");
            CommandLine.WriteLine("=================================================================");

            // Retrieve account list in pages and display data as we receive it.
            string pageToken = null;
            Accounts accountResponse = null;

            do
            {
                var accountRequest = adsense.Accounts.List();
                accountRequest.MaxResults = maxPageSize;
                accountRequest.PageToken = pageToken;
                accountResponse = accountRequest.Fetch();

                if (accountResponse.Items != null && accountResponse.Items.Count > 0)
                {
                    foreach (var account in accountResponse.Items)
                    {
                        CommandLine.WriteLine("Account with ID \"{0}\" and name \"{1}\" was found.",
                            account.Id, account.Name);
                    }
                }
                else
                {
                    CommandLine.WriteLine("No accounts found.");
                }

                pageToken = accountResponse.NextPageToken;

            } while (pageToken != null);

            CommandLine.WriteLine();

            // Return the last page of accounts, so that the main sample has something to run.
            return accountResponse;
        }
    }
}
