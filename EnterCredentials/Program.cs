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
using System.Linq;
using System.Text.RegularExpressions;
using Google.Apis.Samples.Helper;

namespace EnterCredentials
{
    /// <summary>
    /// This is a helper utility which can be used to enter your credentials into all the existing
    /// ClientCredentials.cs files of this project.
    /// </summary>
    internal class Program
    {
        private class CommandLineArguments
        {
            [Argument("id", ShortName = "i", Category="Credentials", Description = "Sets your Client-ID.")]
            public string ClientID { get; set; }

            [Argument("secret", ShortName = "s", Category = "Credentials", Description = "Sets your Client-Secret.")]
            public string ClientSecret { get; set; }

            [Argument("key", ShortName = "k", Category = "Credentials", Description = "Sets your API-Key.")]
            public string ApiKey { get; set; }

            [Argument("remove", ShortName = "r", Category = "Commands", Description = "Removes all credentials.")]
            public bool RemoveCredentials { get; set; }
        }

        static void Main(string[] args)
        {
            // Display the header and parse the command line arguments.
            CommandLine.DisplayGoogleSampleHeader("ClientCredentials.cs Changer");
            CommandLine.WriteAction(" You can find your credentials here:");
            CommandLine.WriteAction("   ^5https://code.google.com/apis/console/b/0/#:access");
            CommandLine.WriteLine();

            var cmdArgs = new CommandLineArguments();
            CommandLineFlags.ParseArguments(cmdArgs, args);

            // Get our values for client id/secret/api key.
            if (cmdArgs.RemoveCredentials)
            {
                CommandLine.WriteAction("Removing credentials from all ClientCredentials.cs ...");
                cmdArgs.ClientID = "<Enter your ClientID here>";
                cmdArgs.ClientSecret = "<Enter your ClientSecret here>";
                cmdArgs.ApiKey = "<Enter your ApiKey here>";
            }
            else
            {
                if (string.IsNullOrEmpty(cmdArgs.ClientID))
                {
                    cmdArgs.ClientID = CommandLine.RequestUserInput<string>("Client ID");
                }
                if (string.IsNullOrEmpty(cmdArgs.ClientSecret))
                {
                    cmdArgs.ClientSecret = CommandLine.RequestUserInput<string>("Client Secret");
                }
                if (string.IsNullOrEmpty(cmdArgs.ApiKey))
                {
                    cmdArgs.ApiKey = CommandLine.RequestUserInput<string>("Api Key");
                }
            }

            // Modifiy all the ClientCredentials.cs files.
            CommandLine.WriteLine();
            CommandLine.WriteAction("Looking for project root ...");
            string root = FindProjectRoot();
            CommandLine.WriteResult("Root directory", root);
            ModifyCredentials(root, cmdArgs);

            CommandLine.PressAnyKeyToExit();
        }

        private static string FindProjectRoot()
        {
            const string SOLUTION = "GoogleApisSamples.sln";
            string dir = Path.GetFullPath(Environment.CurrentDirectory);
            while (!File.Exists(Path.Combine(dir, SOLUTION)))
            {
                dir = Path.GetDirectoryName(dir);

                if (string.IsNullOrEmpty(dir))
                {
                    CommandLine.WriteError("Cannot find sample root directory. Is the {0} file missing?", SOLUTION);
                }
            }
            return dir;
        }

        private static void ModifyCredentials(string dir, CommandLineArguments newData)
        {
            CommandLine.WriteAction("Modifying Credentials ...");
            foreach (string file in Directory.GetFiles(dir, "ClientCredentials.cs", SearchOption.AllDirectories))
            {
                const string pattern = "(public static readonly string )(\\w+)( = \").*(\";)";
                RunLineOperation(file, (srcLine) => Regex.Replace(srcLine, pattern, (m) => ModifyMatch(m, newData)));
                CommandLine.WriteResult("Changed", file);
            }
        }

        private static string ModifyMatch(Match match, CommandLineArguments newData)
        {
            if (!match.Success)
            {
                return match.Groups[0].ToString();
            }
            string fieldName = match.Groups[2].ToString();
            string newValue;
            
            switch (fieldName)
            {
                default:
                    return match.Groups[0].ToString();
                case "ClientID": newValue = newData.ClientID; break;
                case "ClientSecret": newValue = newData.ClientSecret; break;
                case "ApiKey": newValue = newData.ApiKey; break;
            }

            // Escape '"'
            newValue = newValue.Replace("\\", "\\\\").Replace("\"", "\\\"");

            string result = "" + match.Groups[1] + match.Groups[2] + match.Groups[3] + newValue + match.Groups[4];
            return result;
        }


        private static void RunLineOperation(string file, Func<string, string> lineOperation)
        {
            File.WriteAllLines(file, File.ReadAllLines(file).Select(lineOperation).ToArray());
        }
    }
}
