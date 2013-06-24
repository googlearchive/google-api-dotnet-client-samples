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
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;

using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

using Google.Apis.Authentication.OAuth2;
using Google.Apis.Services;
using Google.Apis.Samples.Helper;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util;

namespace Tasks.ASP.NET.SimpleOAuth2
{
    /// <summary>
    /// This sample uses the Tasks service and OAuth2 authentication
    /// to list all of your tasklists and tasks. 
    /// </summary>
    public partial class _Default : System.Web.UI.Page
    {
        private static TasksService _service; // We don't need individual service instances for each client.
        private static OAuth2Authenticator<WebServerClient> _authenticator;
        private IAuthorizationState _state;

        /// <summary>
        /// Returns the authorization state which was either cached or set for this session.
        /// </summary>
        private IAuthorizationState AuthState
        {
            get
            {
                return _state ?? HttpContext.Current.Session["AUTH_STATE"] as IAuthorizationState;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Create the Tasks-Service if it is null.
            if (_service == null)
            {
                _authenticator = CreateAuthenticator();
                _service = new TasksService(new BaseClientService.Initializer()
                    {
                        Authenticator = _authenticator,
                        ApplicationName = "Tasks API Sample",
                    });


            }

            // Check if we received OAuth2 credentials with this request; if yes: parse it.
            if (HttpContext.Current.Request["code"] != null)
            {
                _authenticator.LoadAccessToken();
            }

            // Change the button depending on our auth-state.
            listButton.Text = AuthState == null ? "Authenticate" : "Fetch Tasklists";
        }

        private OAuth2Authenticator<WebServerClient> CreateAuthenticator()
        {
            // Register the authenticator.
            var provider = new WebServerClient(GoogleAuthenticationServer.Description);
            provider.ClientIdentifier = ClientCredentials.ClientID;
            provider.ClientSecret = ClientCredentials.ClientSecret;
            var authenticator =
                new OAuth2Authenticator<WebServerClient>(provider, GetAuthorization) { NoCaching = true };
            return authenticator;
        }

        private IAuthorizationState GetAuthorization(WebServerClient client)
        {
            // If this user is already authenticated, then just return the auth state.
            IAuthorizationState state = AuthState;
            if (state != null)
            {
                return state;
            }

            // Check if an authorization request already is in progress.
            state = client.ProcessUserAuthorization(new HttpRequestInfo(HttpContext.Current.Request));
            if (state != null && (!string.IsNullOrEmpty(state.AccessToken) || !string.IsNullOrEmpty(state.RefreshToken)))
            {
                // Store and return the credentials.
                HttpContext.Current.Session["AUTH_STATE"] = _state = state;
                return state;
            }

            // Otherwise do a new authorization request.
            string scope = TasksService.Scopes.TasksReadonly.GetStringValue();
            OutgoingWebResponse response = client.PrepareRequestUserAuthorization(new[] { scope });
            response.Send(); // Will throw a ThreadAbortException to prevent sending another response.
            return null;
        }

        /// <summary>
        /// Gets the TasksLists of the user.
        /// </summary>
        public void FetchTaskslists()
        {
            try
            {
                // Execute all TasksLists of the user asynchronously.
                TaskLists response = _service.Tasklists.List().Execute();
                ShowTaskslists(response);
            }
            catch (ThreadAbortException)
            {
                // User was not yet authenticated and is being forwarded to the authorization page.
                throw;
            }
            catch (Exception ex)
            {
                output.Text = ex.ToHtmlString();
            }
        }

        private void ShowTaskslists(TaskLists response)
        {
            if (response.Items == null) // If no item is in the response, .Items will be null.
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
            var tasks = _service.Tasks.List(taskList.Id).Execute();
            if (tasks.Items == null)
            {
                return "<i>No items</i>";
            }

            var query = from t in tasks.Items select t.Title;
            return query.Select((str) => "&bull; " + str).Aggregate((a, b) => a + "<br/>" + b);
        }

        protected void listButton_Click(object sender, EventArgs e)
        {
            FetchTaskslists();
        }
    }
}