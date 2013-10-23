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

using Google;
using Google.Apis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util.Store;

namespace Tasks.ETagCollision
{
    /// <summary>
    /// This sample shows the E-Tag collision behavior when an user tries updating an object, which has been modified 
    /// by another source.
    /// </summary>
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Tasks API: E-Tag collision");
            Console.WriteLine("==========================");

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

            // Run the sample code.
            RunSample(service, true, ETagAction.Ignore);
            RunSample(service, true, ETagAction.IfMatch);
            RunSample(service, true, ETagAction.IfNoneMatch);
            RunSample(service, false, ETagAction.Ignore);
            RunSample(service, false, ETagAction.IfMatch);
            RunSample(service, false, ETagAction.IfNoneMatch);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void RunSample(TasksService service, bool modify, ETagAction behaviour)
        {
            Console.WriteLine("Testing for E-Tag case " + behaviour + " with modified=" + modify + "...");

            // Create a new task list.
            TaskList list = service.Tasklists.Insert(new TaskList() { Title = "E-Tag Collision Test" }).Execute();

            // Add a task.
            Task myTask = service.Tasks.Insert(new Task() { Title = "My Task" }, list.Id).Execute();

            // Retrieve a second instance of this task, modify it and commit it.
            if (modify)
            {
                Task myTaskB = service.Tasks.Get(list.Id, myTask.Id).Execute();
                myTaskB.Title = "My Task B!";
                service.Tasks.Update(myTaskB, list.Id, myTaskB.Id).Execute();
            }

            // Modify the original task, and see what happens.
            myTask.Title = "My Task A!";
            var request = service.Tasks.Update(myTask, list.Id, myTask.Id);
            request.ETagAction = behaviour;

            try
            {
                request.Execute();
                Console.WriteLine("\tResult: Success!");
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine("\tResult: Failure! The error message is: " + ex.Message);
            }
            finally
            {
                // Delete the created list. 
                service.Tasklists.Delete(list.Id).Execute();
                Console.WriteLine();
            }
        }
    }
}
