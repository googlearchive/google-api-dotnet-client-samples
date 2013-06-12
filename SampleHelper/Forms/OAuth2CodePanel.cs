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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Google.Apis.Samples.Helper.Forms
{
    /// <summary>
    /// The second panel of the OAuth2Authorization Dialog.
    /// Provides the "Authorization Code" text box.
    /// </summary>
    public partial class OAuth2CodePanel : UserControl
    {
        private const string SuccessRegexPattern = "Success code=([^\\s]+)";
        private const string DeniedRegexPattern = "Denied error=([^\\s]+)";
        private readonly Regex deniedRegex = new Regex(DeniedRegexPattern, RegexOptions.Compiled);
        private readonly Regex successRegex = new Regex(SuccessRegexPattern, RegexOptions.Compiled);

        private bool isClosing;

        public OAuth2CodePanel()
        {
            InitializeComponent();
        }

        public OAuth2CodePanel(Form owner, Uri authUri)
            : this()
        {
            AuthorizationUri = authUri;
            owner.Closed += (sender, args) => Unload();
        }

        /// <summary>
        /// The authorization code entered by the user, or null/empty.
        /// </summary>
        public string AuthorizationCode
        {
            get { return textCode.Text; }
        }

        /// <summary>
        /// The url used for authorization.
        /// </summary>
        public Uri AuthorizationUri { get; private set; }

        /// <summary>
        /// Fired if the entered authorization code changes.
        /// </summary>
        public event EventHandler OnAuthorizationCodeChanged;

        /// <summary>
        /// Fired if a valid authorization code has been entered. Will not fire for user-entered codes.
        /// </summary>
        public event EventHandler OnValidAuthorizationCode;

        /// <summary>
        /// Fired if the authorization request failed. Sender will be an exception object.
        /// </summary>
        public event EventHandler OnAuthorizationError;

        private void OAuth2CodePanel_Load(object sender, EventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += RunCodeGrabber;
            worker.RunWorkerAsync();

            // Register our change event.
            textCode.TextChanged += (textBox, eventArgs) =>
                                        {
                                            if (OnAuthorizationCodeChanged != null)
                                            {
                                                OnAuthorizationCodeChanged(textBox, eventArgs);
                                            }
                                        };

            // Open the browser window.
            OpenRequestBrowserWindow();
        }

        /// <summary>
        /// Unloads this panel.
        /// </summary>
        private void Unload()
        {
            isClosing = true;
        }

        /// <summary>
        /// This method looks at the process list and tries to grab the authorization code.
        /// </summary>
        private void RunCodeGrabber(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000); // Wait until the browser window opens.

            while (!isClosing)
            {
                string code = FindCodeByWindowTitle(true);

                if (!string.IsNullOrEmpty(code))
                {
                    // Code found.
                    isClosing = true;
                    Invoke(
                        new Action(
                            () =>
                            {
                                // Enter the code into the textbox.
                                textCode.Text = code;
                                textCode.Enabled = false;

                                if (OnValidAuthorizationCode != null)
                                {
                                    OnValidAuthorizationCode(this, EventArgs.Empty);
                                }

                                FocusConsoleWindow();
                            }));

                    return;
                }

                // Don't use up all the CPU time.
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Retrieves the authorization code by looking at the window titles of running processes.
        /// </summary>
        /// <param name="minimizeWindow">Defines whether the window should be minimized after it has been found.</param>
        private string FindCodeByWindowTitle(bool minimizeWindow)
        {
            foreach (Process process in Process.GetProcesses())
            {
                string title = process.MainWindowTitle;
                if (string.IsNullOrEmpty(title))
                {
                    continue;
                }

                // If we got an response, fetch the code and return it.
                Match match = successRegex.Match(title);
                if (match.Success)
                {
                    string code = match.Groups[1].ToString();
                    if (minimizeWindow)
                    {
                        MinimizeWindow(process.MainWindowHandle);
                    }
                    return code;
                }

                // Check if we got an error response.
                Match errorMatch = deniedRegex.Match(title);
                if (errorMatch.Success)
                {
                    string error = errorMatch.Groups[1].ToString();
                    if (minimizeWindow)
                    {
                        MinimizeWindow(process.MainWindowHandle);
                    }

                    if (OnAuthorizationError != null)
                    {
                        OnAuthorizationError(
                            new AuthenticationException("Authorization request cancelled: " + error), EventArgs.Empty);
                    }
                }
            }

            return null; // No authorization window was found.
        }

        /// <summary>
        /// Opens the authorization request browser window.
        /// </summary>
        private void OpenRequestBrowserWindow()
        {
            // Let the operation system choose the right browser.
            ThreadPool.QueueUserWorkItem((obj) => Process.Start(AuthorizationUri.ToString()));
        }

        private void bBrowser_Click(object sender, EventArgs e)
        {
            OpenRequestBrowserWindow();
        }

        #region Eye-Candy

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        protected virtual void FocusConsoleWindow()
        {
            // Catch exceptions as chances are high that this operation will fail,
            // and as it is basically just for eye-candy.
            try
            {
                Application.DoEvents();
                SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            }
            catch (InvalidOperationException) { }
            catch (BadImageFormatException) { }
        }

        protected virtual void MinimizeWindow(IntPtr hWnd)
        {
            // Catch exceptions as chances are high that this operation will fail,
            // and as it is basically just for eye-candy.
            try
            {
                Application.DoEvents();
                ShowWindow(hWnd, ShowWindowCommands.ForceMinimized);
            }
            catch (InvalidOperationException) { }
            catch (BadImageFormatException) { }
        }

        /// <summary>Enumeration of the different ways of showing a window using 
        /// ShowWindow</summary>
        private enum ShowWindowCommands : uint
        {
            /// <summary>Hides the window and activates another window.</summary>
            /// <remarks>See SW_HIDE</remarks>
            Hide = 0,
            /// <summary>Activates and displays a window. If the window is minimized 
            /// or maximized, the system restores it to its original size and 
            /// position. An application should specify this flag when displaying 
            /// the window for the first time.</summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1,
            /// <summary>Activates the window and displays it as a minimized window.</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2,
            /// <summary>Minimizes the specified window and activates the next 
            /// top-level window in the Z order.</summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6,
            /// <summary>Displays the window as a minimized window. This value is 
            /// similar to "ShowMinimized", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7,

            ForceMinimized = 11
        }

        #endregion
    }
}