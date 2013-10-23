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
using System.Reflection;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util.Store;

namespace Tasks.ASP.NET.SimpleOAuth2
{
    /// <summary>
    /// This sample uses the Tasks service and OAuth2 authentication to list all of your tasklists and tasks.
    /// Our recommendation is to use ASP.NET MVC applications and Google.Apis.Auth.MVC4 NuGet package.
    /// </summary>
    public partial class _Default : System.Web.UI.Page
    {
        private TasksService service;

        // Application logic should manage users authentication. This sample works with only one user. You can change
        // it by retrieving data from the session.
        private const string UserId = "user-id";

        protected void Page_Load(object sender, EventArgs e)
        {
            GoogleAuthorizationCodeFlow flow;
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Tasks.ASP.NET.SimpleOAuth2.client_secrets.json"))
            {
                flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    DataStore = new FileDataStore("Tasks.ASP.NET.Sample.Store"),
                    ClientSecretsStream = stream,
                    Scopes = new[] { TasksService.Scope.TasksReadonly }
                });
            }

            var uri = Request.Url.ToString();
            var code = Request["code"];
            if (code != null)
            {
                var token = flow.ExchangeCodeForTokenAsync(UserId, code,
                    uri.Substring(0, uri.IndexOf("?")), CancellationToken.None).Result;

                // Extract the right state.
                var oauthState = AuthWebUtility.ExtracRedirectFromState(
                    flow.DataStore, UserId, Request["state"]).Result;
                Response.Redirect(oauthState);
            }
            else
            {
                var result = new AuthorizationCodeWebApp(flow, uri, uri).AuthorizeAsync(UserId,
                    CancellationToken.None).Result;
                if (result.RedirectUri != null)
                {
                    // Redirect the user to the authorization server.
                    Response.Redirect(result.RedirectUri);
                }
                else
                {
                    // The data store contains the user credential, so the user has been already authenticated.
                    service = new TasksService(new BaseClientService.Initializer
                    {
                        ApplicationName = "Tasks API Sample",
                        HttpClientInitializer = result.Credential
                    });
                }
            }
        }

        /// <summary>Gets the TasksLists of the user.</summary>
        public async System.Threading.Tasks.Task FetchTaskslists()
        {
            try
            {
                // Execute all TasksLists of the user asynchronously.
                TaskLists response = await service.Tasklists.List().ExecuteAsync();
                ShowTaskslists(response);
            }
            catch (Exception ex)
            {
                var str = ex.ToString();
                str = str.Replace(Environment.NewLine, Environment.NewLine + "<br/>");
                str = str.Replace("  ", " &nbsp;");
                output.Text = string.Format("<font color=\"red\">{0}</font>", str);
            }
        }

        private void ShowTaskslists(TaskLists response)
        {
            if (response.Items == null)
            {
                output.Text += "You have no task lists!<br/>";
                return;
            }

            output.Text += "Showing task lists...<br/>";
            foreach (TaskList list in response.Items)
            {
                Panel listPanel = new Panel() { BorderWidth = Unit.Pixel(1), BorderColor = Color.Black };
                listPanel.Controls.Add(new Label { Text = list.Title });
                listPanel.Controls.Add(new Label { Text = "<hr/>" });
                listPanel.Controls.Add(new Label { Text = GetTasks(list) });
                lists.Controls.Add(listPanel);
            }
        }

        private string GetTasks(TaskList taskList)
        {
            var tasks = service.Tasks.List(taskList.Id).Execute();
            if (tasks.Items == null)
            {
                return "<i>No items</i>";
            }

            var query = from t in tasks.Items select t.Title;
            return query.Select((str) => "&bull; " + str).Aggregate((a, b) => a + "<br/>" + b);
        }

        protected async void listButton_Click(object sender, EventArgs e)
        {
            await FetchTaskslists();
        }
    }
}