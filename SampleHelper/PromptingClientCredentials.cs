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

namespace Google.Apis.Samples.Helper
{
    public static class PromptingClientCredentials
    {
        private const string ApplicationFolderName = "Google.Apis.Samples";
        private const string ClientCredentialsFileName = "client.dat";
        private const string FileKeyApiKey = "ApiKey";
        private const string FileKeyClientId = "ClientId";
        private const string FileKeyClientSecret = "ClientSecret";
        private const string PromptSimpleCreate = "Blah Blah Blah please create a Simple For First time";


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

        private static IDictionary<string, string> ParseFile()
        {
            var parsedValues = new Dictionary<string, string>(5);
            StreamReader sr = CredentialsFile.OpenText();
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

            return parsedValues;
        }

        private static SimpleClientCredentials CreateSimpleClientCredentials()
        {
            SimpleClientCredentials simpleCredentials =
                CommandLine.CreateClassFromUserinput<SimpleClientCredentials>();
            using (FileStream fStream = CredentialsFile.OpenWrite())
            {
                using (TextWriter tw = new StreamWriter(fStream))
                {
                    tw.WriteLine("{0}={1}", FileKeyApiKey, simpleCredentials.ApiKey);
                }
            }
            return simpleCredentials;
        }

        private static FullClientCredentials CreateFullClientCredentials(bool isExtension)
        {
            FullClientCredentials fullCredentials = CommandLine.CreateClassFromUserinput<FullClientCredentials>();
            using (FileStream fStream = CredentialsFile.OpenWrite())
            {
                using (TextWriter tw = new StreamWriter(fStream))
                {
                    tw.WriteLine("{0}={1}", FileKeyApiKey, fullCredentials.ApiKey);
                    tw.WriteLine("{0}={1}", FileKeyClientId, fullCredentials.ClientId);
                    tw.WriteLine("{0}={1}", FileKeyClientSecret, fullCredentials.ClientSecret);
                }
            }
            return fullCredentials;
        }

        public static SimpleClientCredentials EnsureSimpleClientCredentials()
        {
            if (CredentialsFile.Exists == false)
            {
                return CreateSimpleClientCredentials();
            }

            IDictionary<string, string> values = ParseFile();
            if (values.ContainsKey(FileKeyApiKey) == false)
            {
                return CreateSimpleClientCredentials();
            }
            return new SimpleClientCredentials() { ApiKey = values[FileKeyApiKey] };
        }

        public static FullClientCredentials EnsureFullClientCredentials()
        {
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
                ApiKey = values[FileKeyApiKey],
                ClientId = values[FileKeyClientId], 
                ClientSecret = values[FileKeyClientSecret]};

        }

        public static void ClearClientCredentials()
        {
            FileInfo clientCredentials = CredentialsFile;
            if (clientCredentials.Exists)
            {
                clientCredentials.Delete();
            }
        }
    }

    public class FullClientCredentials : SimpleClientCredentials
    {
        [Description("Client ID as shown in the 'Client ID for installed applications' section")]
        public string ClientId;
        [Description("Client secret as shown in the 'Client ID for installed applications' section")]
        public string ClientSecret;
    }

    public class SimpleClientCredentials
    {
        [Description("API key as shown in the Simple API Access section.")]
        public string ApiKey;
    }
}