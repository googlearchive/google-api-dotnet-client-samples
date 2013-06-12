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
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using DotNetOpenAuth.OAuth2;

using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util;

namespace Tasks.WPF.ListTasks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The remote service on which all the requests are executed.
        /// </summary>
        public static TasksService Service { get; private set; }

        private static IAuthenticator CreateAuthenticator()
        {
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = ClientCredentials.ClientID,
                    ClientSecret = ClientCredentials.ClientSecret
                };
            return new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);
        }

        private static IAuthorizationState GetAuthorization(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.tasks";
            const string KEY = "y},drdzf11x9;87";
            string scope = TasksService.Scopes.Tasks.GetStringValue();

            // Check if there is a cached refresh token available.
            IAuthorizationState state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY);
            if (state != null)
            {
                try
                {
                    client.RefreshToken(state);
                    return state; // Yes - we are done.
                }
                catch (DotNetOpenAuth.Messaging.ProtocolException ex)
                {
                    CommandLine.WriteError("Using existing refresh token failed: " + ex.Message);
                }
            }

            // Retrieve the authorization from the user.
            state = AuthorizationMgr.RequestNativeAuthorization(client, scope);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
            return state;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void UpdateTaskLists()
        {
            // Notice - this is not best practice to create async void method, but for this sample it works.

            // Download a new version of the TaskLists and add the UI controls
            lists.Children.Clear();
            var tasklists = await Service.Tasklists.List().ExecuteAsync();
            foreach (TaskList list in tasklists.Items)
            {
                var tasks = await Service.Tasks.List(list.Id).ExecuteAsync();
                Expander listUI = CreateUITasklist(list, tasks);
                lists.Children.Add(listUI);
            }
        }

        private Expander CreateUITasklist(TaskList list, Google.Apis.Tasks.v1.Data.Tasks tasks)
        {
            var expander = new Expander();

            // Add a bold title.
            expander.Header = list.Title;
            expander.FontWeight = FontWeights.Bold;

            // Add the taskItems (if applicable).
            if (tasks.Items != null)
            {
                var container = new StackPanel();
                foreach (CheckBox box in tasks.Items.Select(CreateUITask))
                {
                    container.Children.Add(box);
                }
                expander.Content = container;
            }
            else
            {
                expander.Content = "There are no tasks in this list.";
            }

            return expander;
        }

        private CheckBox CreateUITask(Task task)
        {
            var checkbox = new CheckBox();
            checkbox.Margin = new Thickness(20, 0, 0, 0);
            checkbox.FontWeight = FontWeights.Normal;
            checkbox.Content = task.Title;
            checkbox.IsChecked = (task.Status == "completed");
            return checkbox;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            // Create the service.
            Service = new TasksService(new BaseClientService.Initializer()
                {
                    Authenticator = CreateAuthenticator(),
                });

            // Get all TaskLists.
            UpdateTaskLists();
        }
    }
}
