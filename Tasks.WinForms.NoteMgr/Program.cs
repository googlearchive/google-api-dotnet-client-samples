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
using System.Windows.Forms;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util;

namespace TasksExample.WinForms.NoteMgr
{
    /// <summary>
    /// Note Manager
    /// A more complex example for the tasks API.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The remote service on which all the requests are executed.
        /// </summary>
        public static TasksService Service { get; private set; }

        private static IAuthenticator CreateAuthenticator()
        {
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description);
            provider.ClientIdentifier = ClientCredentials.ClientID;
            provider.ClientSecret = ClientCredentials.ClientSecret;
            return new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication);
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.tasks";
            const string KEY = "y},drdzf11x9;87";
            string scope = TasksService.Scopes.Tasks.GetStringValue();

            // Check if there is a cached refresh token available.
            IAuthorizationState state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY, scope);
            if (state != null)
            {
                client.RefreshToken(state);
                return state; // Yes - we are done.
            }

            // Retrieve the authorization url:
            state = new AuthorizationState(new[] { scope })
                        { Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl) };
            Uri authUri = client.RequestUserAuthorization(state);

            // Do a new authorization request.
            string authCode = AuthorizationMgr.RequestAuthorization(authUri);
            state = client.ProcessUserAuthorization(authCode, state);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state, scope);
            return state;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the service.
            AuthenticatorFactory.GetInstance().RegisterAuthenticator(CreateAuthenticator);
            Service = new TasksService();
            
            // Open a NoteForm for every task list.
            foreach (TaskList list in Service.Tasklists.List().Fetch().Items)
            {
                // Open a NoteForm.
                new NoteForm(list).Show();
            }
            Application.Run();
        }
    }
}
