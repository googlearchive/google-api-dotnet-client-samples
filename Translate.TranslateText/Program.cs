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
using System.Reflection;
using System.Threading.Tasks;

using Google.Apis.Services;
using Google.Apis.Translate.v2;
using Google.Apis.Translate.v2.Data;
using TranslationsResource = Google.Apis.Translate.v2.Data.TranslationsResource;

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
        #region User Input

        /// <summary>User input for this example.</summary>
        [Description("input")]
        public class TranslateInput
        {
            [Description("text to translate")]
            public string SourceText = "Who ate my candy?";
            [Description("target language")]
            public string TargetLanguage = "fr";
        }

        /// <summary>
        /// Creates a new instance of T and fills all public fields by requesting input from the user.
        /// </summary>
        /// <typeparam name="T">Class with a default constructor</typeparam>
        /// <returns>Instance of T with filled in public fields</returns>
        public static T CreateClassFromUserinput<T>()
        {
            var type = typeof(T);

            // Create an instance of T
            T settings = Activator.CreateInstance<T>();

            Console.WriteLine("Please enter values for the {0}:", GetDescriptiveName(type));

            // Fill in parameters
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(settings);

                // Let the user input a value
                RequestUserInput(GetDescriptiveName(field), ref value, field.FieldType);

                field.SetValue(settings, value);
            }

            Console.WriteLine();
            return settings;
        }

        /// <summary>
        /// Tries to return a descriptive name for the specified member info. It uses the DescriptionAttribute if 
        /// available.
        /// </summary>
        /// <returns>Description from DescriptionAttriute or name of the MemberInfo</returns>
        public static string GetDescriptiveName(MemberInfo info)
        {
            // If available, return the description set in the DescriptionAttribute.
            foreach (DescriptionAttribute attribute in info.GetCustomAttributes(typeof(DescriptionAttribute), true))
            {
                return attribute.Description;
            }

            // Otherwise: return the name of the member.
            return info.Name;
        }

        /// <summary>Requests an user input for the specified value.</summary>
        /// <param name="name">Name to display.</param>
        /// <param name="value">Default value, and target value.</param>
        /// <param name="valueType">Type of the target value.</param>
        private static void RequestUserInput(string name, ref object value, Type valueType)
        {
            do
            {
                Console.Write("\t{0}: ", name);
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    // No change required, use default value.
                    return;
                }

                try
                {
                    value = Convert.ChangeType(input, valueType);
                    return;
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine("Please enter a valid value!");
                }
            } while (true); // Run this loop until the user gives a valid input.
        }

        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Translate Sample");
            Console.WriteLine("================");

            try
            {
                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Run()
        {
            var key = GetApiKey();

            // Ask for the user input.
            TranslateInput input = CreateClassFromUserinput<TranslateInput>();

            // Create the service.
            var service = new TranslateService(new BaseClientService.Initializer()
                {
                    ApiKey = key,
                    ApplicationName = "Translate API Sample"
                });

            // Execute the first translation request.
            Console.WriteLine("Translating to '" + input.TargetLanguage + "' ...");

            string[] srcText = new[] { "Hello world!", input.SourceText };
            var response = await service.Translations.List(srcText, input.TargetLanguage).ExecuteAsync();
            var translations = new List<string>();

            foreach (TranslationsResource translation in response.Translations)
            {
                translations.Add(translation.TranslatedText);
                Console.WriteLine("translation :" + translation.TranslatedText);
            }

            // Translate the text (back) to English.
            Console.WriteLine("Translating to English ...");

            response = service.Translations.List(translations, "en").Execute();
            foreach (TranslationsResource translation in response.Translations)
            {
                Console.WriteLine("translation :" + translation.TranslatedText);
            }
        }

        private static string GetApiKey()
        {
            Console.WriteLine("Enter API Key");
            return Console.ReadLine();
        }
    }
}
