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
using System.IO;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util.Store;

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
            Console.WriteLine("Tasks OAuth2 Sample");
            Console.WriteLine("===================");

            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { TasksService.Scope.Tasks },
                    "user", CancellationToken.None, new FileDataStore("Tasks.Auth.Store")).Result;
            }

            // Create the service.
            var service = new TasksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Tasks API Sample",
            });

            TaskLists results = service.Tasklists.List().Execute();
            Console.WriteLine("\tLists:");

            foreach (TaskList list in results.Items)
            {
                Console.WriteLine("\t\t" + list.Title);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
