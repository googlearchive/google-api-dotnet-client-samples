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
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util;

namespace TasksSample.CreateTasks
{
    /// <summary>
    /// Tasks API sample using OAuth2.
    /// This sample demonstrates how to use OAuth2 and how to request an access code.
    /// The application will only ask you to grant access to this sample once, even when run multiple times.
    /// </summary>
    internal class Program
    {
        private const string SampleListName = ".NET Tasks API Example";
        private static readonly string Scope = TasksService.Scopes.Tasks.GetStringValue();

        public static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Tasks API");

            // Register the authenticator.
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description);
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            provider.ClientIdentifier = credentials.ClientId;
            provider.ClientSecret = credentials.ClientSecret;
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);

            // Create the service.
            var service = new TasksService(auth);

            // Execute request: Create sample list.
            if (!ListExists(service, SampleListName) &&
                CommandLine.RequestUserChoice("Do you want to create a sample list?"))
            {
                CreateSampleTasklist(service);
            }
            CommandLine.WriteLine();

            // Execute request: List task-lists.
            ListTaskLists(service);

            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthorization(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.siteverification";
            const string KEY = "y},drdzf11x9;87";

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
            state = AuthorizationMgr.RequestNativeAuthorization(client, Scope);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
            return state;
        }

        private static bool ListExists(TasksService service, string list)
        {
            return
                (from TaskList taskList in service.Tasklists.List().Fetch().Items
                 where taskList.Title == list
                 select taskList).Count() > 0;
        }

        private static void CreateSampleTasklist(TasksService service)
        {
            var list = new TaskList();
            list.Title = SampleListName;
            list = service.Tasklists.Insert(list).Fetch();

            service.Tasks.Insert(new Task { Title = "Test the Tasklist API" }, list.Id).Fetch();
            service.Tasks.Insert(new Task { Title = "Do the laundry" }, list.Id).Fetch();
        }

        private static void ListTaskLists(TasksService service)
        {
            CommandLine.WriteLine("   ^1Task lists:");
            var list = service.Tasklists.List().Fetch();
            foreach (var item in list.Items)
            {
                CommandLine.WriteLine("     ^2" + item.Title);

                Tasks tasks = service.Tasks.List(item.Id).Fetch();
                if (tasks.Items != null)
                {
                    foreach (Task t in tasks.Items)
                    {
                        CommandLine.WriteLine("        ^4" + t.Title);
                    }
                }
            }
        }
    }
}