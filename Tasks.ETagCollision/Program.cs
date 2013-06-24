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


using DotNetOpenAuth.OAuth2;

using Google;
using Google.Apis;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util;

namespace Tasks.ETagCollision
{
    /// <summary>
    /// This sample shows the E-Tag collision behaviour when an user tries updating an object, 
    /// which has been modified by another source.
    /// </summary>
    internal class Program
    {
        private static readonly string Scope = TasksService.Scopes.Tasks.GetStringValue();

        public static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Tasks API: E-Tag collision");

            // Register the authenticator.
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                };

            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication);

            // Create the service.
            var service = new TasksService(new BaseClientService.Initializer()
                {
                    Authenticator = auth,
                    ApplicationName = "Tasks API Sample",
                });

            // Run the sample code.
            RunSample(service, true, ETagAction.Ignore);
            RunSample(service, true, ETagAction.IfMatch);
            RunSample(service, true, ETagAction.IfNoneMatch);
            RunSample(service, false, ETagAction.Ignore);
            RunSample(service, false, ETagAction.IfMatch);
            RunSample(service, false, ETagAction.IfNoneMatch);
            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
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

        private static void RunSample(TasksService service, bool modify, ETagAction behaviour)
        {
            CommandLine.WriteAction("Testing for E-Tag case " + behaviour + " with modified=" + modify + "...");

            // Create a new task list.
            TaskList list = service.Tasklists.Insert(new TaskList() { Title = "E-Tag Collision Test" }).Execute();

            // Add a task
            Task myTask = service.Tasks.Insert(new Task() { Title = "My Task" }, list.Id).Execute();

            // Retrieve a second instance of this task, modify it and commit it
            if (modify)
            {
                Task myTaskB = service.Tasks.Get(list.Id, myTask.Id).Execute();
                myTaskB.Title = "My Task B!";
                service.Tasks.Update(myTaskB, list.Id, myTaskB.Id).Execute();
            }

            // Modfiy the original task, and see what happens
            myTask.Title = "My Task A!";
            var request = service.Tasks.Update(myTask, list.Id, myTask.Id);
            request.ETagAction = behaviour;

            try
            {
                request.Execute();
                CommandLine.WriteResult("Result", "Success!");
            }
            catch (GoogleApiException ex)
            {
                CommandLine.WriteResult(
                    "Result", "Failure! (" + ex.Message + ")");
            }
            finally
            {
                // Delete the created list. 
                service.Tasklists.Delete(list.Id).Execute();
                CommandLine.WriteLine();
            }
        }
    }
}
