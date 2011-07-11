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

namespace Google.Apis.Samples.Helper.Forms
{
    /// <summary>
    /// OAuth2 authorization dialog which provides the native application authorization flow.
    /// </summary>
    public partial class OAuth2AuthorizationDialog : Form
    {
        /// <summary>
        /// The URI used for user-authentication.
        /// </summary>
        public Uri AuthorizationUri { get; set; }

        /// <summary>
        /// The authorization code retrieved from the user.
        /// </summary>
        public string AuthorizationCode { get; private set; }

        /// <summary>
        /// The authorization error (if any occured), or null.
        /// </summary>
        public string AuthorizationError { get; private set; }

        public OAuth2AuthorizationDialog()
        {
            InitializeComponent();
            content.Controls.Add(new OAuth2IntroPanel());
            AuthorizationCode = null;
        }

        /// <summary>
        /// Shows the authorization form and uses the specified URL for authorization.
        /// </summary>
        /// <returns>The authorization code.</returns>
        public static string ShowDialog(Uri authUri)
        {
            var dialog = new OAuth2AuthorizationDialog { AuthorizationUri = authUri };
            dialog.ShowDialog();
            return dialog.AuthorizationCode;
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OAuth2AuthorizationDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthorizationCode) || !string.IsNullOrEmpty(AuthorizationError))
            {
                return; // We are done here.
            }

            // Display a "Are you sure?" message box to the user.
            DialogResult result =
                MessageBox.Show(
                    "This application cannot continue without your authorization. Are you sure you want to " +
                    "cancel the authorization request?", "Are you sure?", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                // The user doesn't want to close the form anymore.
                e.Cancel = true;
            }
        }

        private void bNext_Click(object sender, EventArgs e)
        {
            if (content.Controls[0] is OAuth2IntroPanel)
            {
                // We are on the first page. Move to the next one.
                content.Controls.Clear();
                bNext.Enabled = false; // Disable the next button as long as no code has been entered.
                var nextPanel = new OAuth2CodePanel(this, AuthorizationUri);
                nextPanel.OnAuthorizationCodeChanged += (textBox, eventArgs) =>
                                                            {
                                                                bNext.Enabled =
                                                                    !string.IsNullOrEmpty(nextPanel.AuthorizationCode);
                                                            };
                nextPanel.OnValidAuthorizationCode += bNext_Click;
                nextPanel.OnAuthorizationError +=
                    (exception, eventArgs) => OnAuthenticationError(exception as Exception);
                content.Controls.Add(nextPanel);
            }
            else if (content.Controls[0] is OAuth2CodePanel)
            {
                AuthorizationCode = ((OAuth2CodePanel) content.Controls[0]).AuthorizationCode;
                Close();
            }
        }

        private void OnAuthenticationError(Exception exception)
        {
            MessageBox.Show(exception.Message,
                "Authentication request failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AuthorizationError = exception.Message;
            Invoke(new Action(Close));
        }
    }
}
