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
using Google.Apis.Adsense.v1_1;
using Google.Apis.Adsense.v1_1.Data;
using Google.Apis.Samples.Helper;

namespace AdSense.Sample
{
    /// <summary>
    /// This example gets a specific account for the logged in user.
    /// This includes the full tree of sub-accounts.
    ///
    /// Tags: accounts.get
    /// </summary>
    class GetAccountTree
    {
        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <param name="adsense">AdSense service object on which to run the requests.</param>
        /// <param name="accountId">The ID for the account to be used.</param>
        public static void Run(AdsenseService adsense, string accountId)
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Displaying AdSense account tree for {0}", accountId);
            CommandLine.WriteLine("=================================================================");

            // Retrieve account.
            var account = adsense.Accounts.Get(accountId).Fetch();
            displayTree(account, 0);

            CommandLine.WriteLine();
        }

        /// <summary>
        /// Auxiliary method to recurse through the account tree, displaying it
        /// </summary>
        /// <param name="parentAccount">The account to print a sub-tree for.</param>
        /// <param name="level">The depth at which the top account exists in the tree.</param>
        private static void displayTree(Account parentAccount, int level)
        {
            CommandLine.WriteLine("{0}Account with ID \"{1}\" and name \"{2}\" was found.",
                new String(' ', 2 * level), parentAccount.Id, parentAccount.Name);

            var subAccounts = parentAccount.SubAccounts;

            if (subAccounts != null)
            {
                foreach (var subAccount in subAccounts)
                {
                    displayTree(subAccount, level + 1);
                }
            }
        }
    }
}
