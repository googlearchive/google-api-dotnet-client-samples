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
using System.Linq;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util.Store;

namespace TasksSample.CreateTasks
{
    /// <summary>
    /// Tasks API sample using OAuth2.
    /// This sample demonstrates how to use OAuth2 and how to request an access code.
    /// The application will only ask you to grant access to this sample once, even when run multiple times.
    /// </summary>
    internal class Program
    {
        private const string SampleListName = "Sample List";

        public static void Main(string[] args)
        {
            Console.WriteLine(".NET Tasks API Sample");
            Console.WriteLine("=====================");

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

            // Execute request: Create sample list.
            if (!ListExists(service, SampleListName) && CreateSampleList())
            {
                CreateSampleTasklist(service);
            }
            Console.WriteLine();

            // Execute request: List task-lists.
            ListTaskLists(service);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>Asks the user if he wants to create a sample list.</summary>
        /// <returns><c>true</c> if the user to press 'y' or 'yes' to continue</returns>
        private static bool CreateSampleList()
        {
            Console.WriteLine("Do you want to create a sample list?");

            var yesOptions = new[] { "y", "yes" };
            var noOptions = new[] { "n", "no" };

            string input;
            do
            {
                Console.WriteLine("Press 'y' | 'yes' to continue, or 'n' | 'no' to stop");
                input = Console.ReadLine().ToLower();
            } while (!yesOptions.Contains(input) && !noOptions.Contains(input));

            return yesOptions.Contains(input);
        }

        private static bool ListExists(TasksService service, string list)
        {
            return (from TaskList taskList in service.Tasklists.List().Execute().Items
                    where taskList.Title == list
                    select taskList).Count() > 0;
        }

        private static void CreateSampleTasklist(TasksService service)
        {
            var list = new TaskList();
            list.Title = SampleListName;
            list = service.Tasklists.Insert(list).Execute();

            service.Tasks.Insert(new Task { Title = "Learn the Task API" }, list.Id).Execute();
            service.Tasks.Insert(new Task { Title = "Implement a WP application using Task API" }, list.Id).Execute();
        }

        private static void ListTaskLists(TasksService service)
        {
            Console.WriteLine("\tTask lists:");
            var list = service.Tasklists.List().Execute();
            foreach (var item in list.Items)
            {
                Console.WriteLine("\t\t" + item.Title);
                Tasks tasks = service.Tasks.List(item.Id).Execute();
                if (tasks.Items != null)
                {
                    foreach (Task t in tasks.Items)
                    {
                        Console.WriteLine("\t\t\t" + t.Title);
                    }
                }
            }
        }
    }
}