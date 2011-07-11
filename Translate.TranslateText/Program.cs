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
using Google.Apis;
using Google.Apis.Data;
using Google.Apis.Samples.Helper;
using TranslationsResource = Google.Apis.Data.TranslationsResource;

namespace Translate.TranslateText
{
    /// <summary>
    /// This example uses the Translate API to translate a user 
    /// entered phrase from English to French or a language of the user's choice.
    /// 
    /// Uses your DeveloperKey for authentication.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// User input for this example.
        /// </summary>
        [Description("input")]
        public class ServiceDescription
        {
            [Description("text to translate")] public string SourceText = "Who ate my candy?";
            [Description("target language")] public string TargetLanguage = "fr";
        }

        [STAThread]
        static void Main(string[] args)
        {
            // Initialize this sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Translate Sample");

            // Ask for the user input.
            ServiceDescription input = CommandLine.CreateClassFromUserinput<ServiceDescription>();

            // Create the service.
            var service = new TranslateService() { DeveloperKey = ClientCredentials.DeveloperKey };

            // Execute the first translation request.
            CommandLine.WriteAction("Translating to '"+input.TargetLanguage+"' ...");

            string[] srcText = new[] { "Hello world!", input.SourceText };
            TranslationsListResponse response = service.Translations.List(srcText, input.TargetLanguage).Fetch();
            var translations = new List<string>();

            foreach (TranslationsResource translation in response.Translations)
            {
                translations.Add(translation.TranslatedText);
                CommandLine.WriteResult("translation", translation.TranslatedText);
            }

            // Translate the text (back) to english.
            CommandLine.WriteAction("Translating to english ...");

            response = service.Translations.List(translations, "en").Fetch();

            foreach (TranslationsResource translation in response.Translations)
            {
                CommandLine.WriteResult("translation", translation.TranslatedText);
            }

            // ...and we are done.
            CommandLine.PressAnyKeyToExit();
        }
    }
}
