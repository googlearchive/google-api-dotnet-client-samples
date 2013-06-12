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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Samples.Helper.Properties;

namespace Google.Apis.Samples.Helper.NativeAuthorizationFlows
{
    /// <summary>
    /// A native authorization flow which uses a listening local loopback socket to fetch the authorization code.
    /// </summary>
    /// <remarks>Might not work if blocked by the system firewall.</remarks>
    public class LoopbackServerAuthorizationFlow : INativeAuthorizationFlow
    {
        private const string LoopbackCallback = "http://localhost:{0}/{1}/authorize/";

        /// <summary>
        /// Returns a random, unused port.
        /// </summary>
        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            try
            {
                listener.Start();
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }

        /// <summary>
        /// Handles an incoming WebRequest.
        /// </summary>
        /// <param name="context">The request to handle.</param>
        /// <param name="appName">Name of the application handling the request.</param>
        /// <returns>The authorization code, or null if the process was cancelled.</returns>
        private string HandleRequest(HttpListenerContext context)
        {
            try
            {
                // Check whether we got a successful response:
                string code = context.Request.QueryString["code"];
                if (!string.IsNullOrEmpty(code))
                {
                    return code;
                }

                // Check whether we got an error response:
                string error = context.Request.QueryString["error"];
                if (!string.IsNullOrEmpty(error))
                {
                    return null; // Request cancelled by user.
                }

                // The response is unknown to us. Choose a different authentication flow.
                throw new NotSupportedException(
                    "Received an unknown response: " + Environment.NewLine + context.Request.RawUrl);
            }
            finally
            {
                // Write a response.
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    string response = Resources.LoopbackServerHtmlResponse.Replace("{APP}", Util.ApplicationName);
                    writer.WriteLine(response);
                    writer.Flush();
                }
                context.Response.OutputStream.Close();
                context.Response.Close();
            }
        }

        public string RetrieveAuthorization(UserAgentClient client, IAuthorizationState authorizationState)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("HttpListener is not supported by this platform.");
            }

            // Create a HttpListener for the specified url.
            string url = string.Format(LoopbackCallback, GetRandomUnusedPort(), Util.ApplicationName);
            authorizationState.Callback = new Uri(url);
            var webserver = new HttpListener();
            webserver.Prefixes.Add(url);

            // Retrieve the authorization url.
            Uri authUrl = client.RequestUserAuthorization(authorizationState);

            try
            {
                // Start the webserver.
                webserver.Start();

                // Open the browser.
                Process.Start(authUrl.ToString());

                // Wait for the incoming connection, then handle the request.
                return HandleRequest(webserver.GetContext());
            }
            catch (HttpListenerException ex)
            {
                throw new NotSupportedException("The HttpListener threw an exception.", ex);
            }
            finally
            {
                // Stop the server after handling the one request.
                webserver.Stop();
            }
        }
    }
}
