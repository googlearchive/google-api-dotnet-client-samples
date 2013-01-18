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

using Google.Apis.Dfareporting.v1_1;
using Google.Apis.Dfareporting.v1_1.Data;

using Google.Apis.Samples.Helper;

namespace DfaReporting.Sample
{
    /// <summary>
    /// Lists all DFA user profiles associated with your Google Account.
    /// </summary>
    internal class GetAllUserProfilesHelper
    {
        private readonly DfareportingService service;

        /// <summary>
        /// Instantiate a helper for listing the DFA user profiles associated with your Google Account.
        /// </summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        public GetAllUserProfilesHelper(DfareportingService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Runs this sample.
        /// </summary>
        /// <returns>The list of user profiles received.</returns>
        public UserProfileList Run()
        {
            CommandLine.WriteLine("=================================================================");
            CommandLine.WriteLine("Listing all DFA user profiles");
            CommandLine.WriteLine("=================================================================");

            // Retrieve DFA user profiles and display them. User profiles do not support
            // paging.

            var profiles = service.UserProfiles.List().Fetch();
            if (profiles.Items.Count > 0)
            {
                foreach (var profile in profiles.Items)
                {
                    CommandLine.WriteLine("User profile with ID \"{0}\" and name \"{1}\" was found.",
                        profile.ProfileId, profile.UserName);
                }
            }
            else
            {
                CommandLine.WriteLine("No profiles found.");
            }

            CommandLine.WriteLine();
            return profiles;
        }
    }
}
