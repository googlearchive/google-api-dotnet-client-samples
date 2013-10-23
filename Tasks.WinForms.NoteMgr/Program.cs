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
using System.Windows.Forms;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;

namespace TasksExample.WinForms.NoteMgr
{
    /// <summary>A note manager - A more complex example for the tasks API.</summary>
    internal static class Program
    {
        /// <summary>The remote service on which all the requests are executed.</summary>
        public static TasksService Service { get; private set; }

        /// <summary>The main entry point for the application.</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                GoogleWebAuthorizationBroker.Folder = "Tasks.Auth.Store";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { TasksService.Scope.Tasks },
                    "user", CancellationToken.None).Result;
            }

            // Initialize the service.
            Service = new TasksService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Tasks API Sample"
                });

            // Open a NoteForm for every task list.
            foreach (TaskList list in Service.Tasklists.List().Execute().Items)
            {
                // Open a NoteForm.
                new NoteForm(list).Show();
            }
            Application.Run();
        }
    }
}
