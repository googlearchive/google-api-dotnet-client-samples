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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// This class is for use of samples only, on first use this class prompts the user for
    /// ApiKey and optionaly ClientId and ClientSecret, these are stored encrypted in a file.
    /// This is not sutible for production use as the user can access these keys.
    /// </summary>
    public static class PromptingClientCredentials
    {
        private static bool firstRun = true;
        private const string ApplicationFolderName = "Google.Apis.Samples";
        private const string ClientCredentialsFileName = "client.dat";
        private const string FileKeyApiKey = "ProtectedApiKey";
        private const string FileKeyClientId = "ProtectedClientId";
        private const string FileKeyClientSecret = "ProtectedClientSecret";

        // Random data used to make this encryption key different from other information encyrpted with ProtectedData
        // This does not make it hard to decrypt just adds another small step.  
        private static readonly byte[] entropy = new byte[] { 
            150, 116, 112, 35, 243, 210, 144, 9, 188, 122, 157, 253, 124, 115, 87, 51, 84, 178, 43, 176, 239, 198, 198, 
            249, 116, 190, 61, 129, 238, 23, 250, 163, 59, 26, 139 };
        
        private const string PromptCreate = "This looks like the first time your running the Google(tm) API " +
            "Samples if you have already got your API key please enter it here (you can find your key " +
            "at https://code.google.com/apis/console/#:access). Otherwise " +
            "please follow the instructions at http://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted " +
            " look out for the API Console section. " + 
            "This will be stored encrypted on the hard drive so that only this user can access these keys.";
        private const string PromptSimpleCreate = PromptCreate + 
            " For the sample you are running you need just need API Key.";
        private const string PromptFullCreate = PromptCreate + 
            " For the sample you are running you need both an API Key and " + 
            "a Client ID for installed applications.";
        private const string PromptFullExtend = 
            "Another sample? Cool! This one requires ClientId for Installed applications " +
            " as well as the API Key you entered earlier. You can pick up your new ClientId from " +
            "https://code.google.com/apis/console/#:access";
        
        
        /// <summary>Gives a fileInfo pointing to the CredentialsFile, creating directories if required.</summary>
        private static FileInfo CredentialsFile
        {
            get
            {
                string applicationDate = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string googleAppDirectory = Path.Combine(applicationDate, ApplicationFolderName);
                var directoryInfo = new DirectoryInfo(googleAppDirectory);
                if (directoryInfo.Exists == false)
                {
                    directoryInfo.Create();
                }
                return new FileInfo(Path.Combine(googleAppDirectory, ClientCredentialsFileName));
            }
        }

        /// <summary>
        ///     Returns a IDictionary of keys and values from the CredentialsFile which is expected to be of the form
        ///     <example>
        ///       key=value
        ///       key2=value2
        ///     </example>
        /// </summary>
        private static IDictionary<string, string> ParseFile()
        {
            var parsedValues = new Dictionary<string, string>(5);
            using (StreamReader sr = CredentialsFile.OpenText())
            {
                string currentLine = sr.ReadLine();
                while (currentLine != null)
                {
                    int firstEquals = currentLine.IndexOf('=');
                    if (firstEquals > 0 && firstEquals + 1 < currentLine.Length)
                    {
                        string key = currentLine.Substring(0, firstEquals);
                        string value = currentLine.Substring(firstEquals + 1);
                        parsedValues.Add(key, value);
                    }
                    currentLine = sr.ReadLine();
                }
            }

            return parsedValues;
        }

        /// <summary>
        /// By prompting the user this constructs <code>SimpleClientCredentials</code> and stores them in the 
        /// <code>CredentialsFile</code>
        /// </summary>
        private static SimpleClientCredentials CreateSimpleClientCredentials()
        {
            CommandLine.WriteLine(PromptSimpleCreate);
            SimpleClientCredentials simpleCredentials =
                CommandLine.CreateClassFromUserinput<SimpleClientCredentials>();
            using (FileStream fStream = CredentialsFile.OpenWrite())
            {
                using (TextWriter tw = new StreamWriter(fStream))
                {
                    tw.WriteLine("{0}={1}", FileKeyApiKey, Protect(simpleCredentials.ApiKey));
                }
            }
            return simpleCredentials;
        }

        /// <summary>
        /// By prompting the user this constructs <code>FullClientCredentials</code> and stores them in the 
        /// <code>CredentialsFile</code>
        /// </summary>
        private static FullClientCredentials CreateFullClientCredentials(bool isExtension)
        {
            CommandLine.WriteLine(isExtension ? PromptFullExtend : PromptFullCreate);

            FullClientCredentials fullCredentials = CommandLine.CreateClassFromUserinput<FullClientCredentials>();
            using (FileStream fStream = CredentialsFile.OpenWrite())
            {
                using (TextWriter tw = new StreamWriter(fStream))
                {
                    tw.WriteLine("{0}={1}", FileKeyApiKey, Protect(fullCredentials.ApiKey));
                    tw.WriteLine("{0}={1}", FileKeyClientId, Protect(fullCredentials.ClientId));
                    tw.WriteLine("{0}={1}", FileKeyClientSecret, Protect(fullCredentials.ClientSecret));
                }
            }
            return fullCredentials;
        }

        /// <summary>
        /// Encrypts the clearText using the current users key, this prevents other users being able to read this
        /// but does not stop the current user from reading this.
        /// </summary>
        private static string Protect(string clearText)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.ASCII.GetBytes(clearText), entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// The inverse of <code>Protect</code> this decrypts the passed-in string.
        /// </summary>
        private static string Unprotect(string encrypted)
        {
            byte[] encryptedData = Convert.FromBase64String(encrypted);
            byte[] clearText = ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.CurrentUser);
            return Encoding.ASCII.GetString(clearText);
        }

        private static void PromptForReuse()
        {
            if ((!firstRun) || (!CredentialsFile.Exists))
            {
                return;
            }
            firstRun = false;
            CommandLine.RequestUserChoice(
                "There are stored API Keys on this computer do you wish to use these or enter new credentials?",
                new UserOption("Reuse existing API Keys", () => { ;}),
                new UserOption("Enter new credentials", ClearClientCredentials));
        }

        /// <summary>
        /// Fetches the users ApiKey either from local disk or prompts the user in the command line.
        /// </summary>
        public static SimpleClientCredentials EnsureSimpleClientCredentials()
        {
            PromptForReuse();
            if (CredentialsFile.Exists == false)
            {
                return CreateSimpleClientCredentials();
            }

            IDictionary<string, string> values = ParseFile();
            if (values.ContainsKey(FileKeyApiKey) == false)
            {
                return CreateSimpleClientCredentials();
            }
            return new SimpleClientCredentials() { ApiKey = Unprotect(values[FileKeyApiKey]) };
        }

        /// <summary>
        /// Fetches the users ApiKey, ClientId and ClientSecreat either from local disk or 
        /// prompts the user in the command line.
        /// </summary>
        public static FullClientCredentials EnsureFullClientCredentials()
        {
            PromptForReuse();
            if (CredentialsFile.Exists == false)
            {
                return CreateFullClientCredentials(false);
            }

            IDictionary<string, string> values = ParseFile();
            if (values.ContainsKey(FileKeyApiKey) == false || 
                values.ContainsKey(FileKeyClientId) == false ||
                values.ContainsKey(FileKeyClientSecret) == false)
            {
                return CreateFullClientCredentials(true);
            }

            return new FullClientCredentials() { 
                ApiKey = Unprotect(values[FileKeyApiKey]),
                ClientId = Unprotect(values[FileKeyClientId]), 
                ClientSecret = Unprotect(values[FileKeyClientSecret])};

        }

        /// <summary>
        /// Removes the stored credentials from this computer
        /// </summary>
        public static void ClearClientCredentials()
        {
            FileInfo clientCredentials = CredentialsFile;
            if (clientCredentials.Exists)
            {
                clientCredentials.Delete();
            }
        }
    }

    /// <summary>Simple DTO holding all the credentials required to work with the Google Api</summary>
    public class FullClientCredentials : SimpleClientCredentials
    {
        [Description("Client ID as shown in the 'Client ID for installed applications' section")]
        public string ClientId;
        [Description("Client secret as shown in the 'Client ID for installed applications' section")]
        public string ClientSecret;
    }

    /// <summary>Simple DTO holding a minimal set of credentials required to work with the Google Api</summary>
    public class SimpleClientCredentials
    {
        [Description("API key as shown in the Simple API Access section.")]
        public string ApiKey;
    }
}