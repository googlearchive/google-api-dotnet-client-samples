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

using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// General Utility class for samples.
    /// </summary>
    public class Util
    {
        /// <summary>
        /// Returns the name of the application currently being run.
        /// </summary>
        public static string ApplicationName
        {
            get { return Assembly.GetEntryAssembly().GetName().Name; }
        }

        /// <summary>
        /// Tries to retrieve and return the content of the clipboard. Will trim the content to the specified length.
        /// Removes all new line characters from the input.
        /// </summary>
        /// <remarks>Requires the STAThread attribute on the Main method.</remarks>
        /// <returns>Trimmed content of the clipboard, or null if unable to retrieve.</returns>
        public static string GetSingleLineClipboardContent(int maxLen)
        {
            try
            {
                string text = Clipboard.GetText().Replace("\r", "").Replace("\n", "");
                if (text.Length > maxLen)
                {
                    return text.Substring(0, maxLen);
                }
                return text;
            }
            catch (ExternalException)
            {
                return null; // Something is preventing us from getting the clipboard content -> return.
            }
        }

        /// <summary>
        /// Changes the clipboard content to the specified value.
        /// </summary>
        /// <remarks>Requires the STAThread attribute on the Main method.</remarks>
        /// <param name="text"></param>
        public static void SetClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (ExternalException) {}
        }
    }
}
