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
using System.Windows.Forms;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Samples.Helper.Forms;

namespace Google.Apis.Samples.Helper.NativeAuthorizationFlows
{
    /// <summary>
    /// Describes a flow which captures the authorization code out of the window title of the browser.
    /// </summary>
    /// <remarks>Works on Windows, but not on Unix. Will failback to copy/paste mode if unsupported.</remarks>
    internal class WindowTitleNativeAuthorizationFlow : INativeAuthorizationFlow
    {
        private const string OutOfBandCallback = "urn:ietf:wg:oauth:2.0:oob";

        public string RetrieveAuthorization(UserAgentClient client, IAuthorizationState authorizationState)
        {
            // Create the Url.
            authorizationState.Callback = new Uri(OutOfBandCallback);
            Uri url = client.RequestUserAuthorization(authorizationState);

            // Show the dialog.
            if (!Application.RenderWithVisualStyles)
            {
                Application.EnableVisualStyles();
            }

            Application.DoEvents();
            string authCode = OAuth2AuthorizationDialog.ShowDialog(url);
            Application.DoEvents();

            if (string.IsNullOrEmpty(authCode))
            {
                return null; // User cancelled the request.
            }

            return authCode;
        }
    }
}
