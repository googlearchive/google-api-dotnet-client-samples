/*
Copyright 2011 Google Inc

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
using System.Diagnostics;
using System.Linq;

using DotNetOpenAuth.OAuth2;

using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Discovery;
using Google.Apis.Samples.Helper;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util;

namespace Google.Apis.Samples.TasksOAuth2
{
    /// <summary>
    /// This sample demonstrates the simplest use case for an OAuth2 service. 
    /// The schema provided here can be applied to every request requiring authentication.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Tasks API");

            // Register the authenticator.
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                };
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);

            // Create the service.
            var service = new TasksService(new BaseClientService.Initializer()
                {
                    Authenticator = auth
                });
            TaskLists results = service.Tasklists.List().Fetch();
            CommandLine.WriteLine("   ^1Lists:");

            foreach (TaskList list in results.Items)
            {
                CommandLine.WriteLine("     ^2" + list.Title);
            }

            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthorization(NativeApplicationClient arg)
        {
            // Get the auth URL:
            IAuthorizationState state = new AuthorizationState(new[] { TasksService.Scopes.Tasks.GetStringValue() });
            state.Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl);
            Uri authUri = arg.RequestUserAuthorization(state);

            // Request authorization from the user (by opening a browser window):
            Process.Start(authUri.ToString());
            Console.Write("  Authorization Code: ");
            string authCode = Console.ReadLine();
            Console.WriteLine();

            // Retrieve the access token by using the authorization code:
            return arg.ProcessUserAuthorization(authCode, state);
        }
    }
}
