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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Prediction.v1_3;
using Google.Apis.Prediction.v1_3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Prediction.HostedExample
{
    /// <summary>
    /// Sample for the prediction API.
    /// This sample makes use of the predefined "Language Identifier" demo prediction set.
    /// http://code.google.com/apis/predict/docs/gallery.html
    /// </summary>
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Prediction API");
            Console.WriteLine("==============");

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
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { PredictionService.Scope.Prediction },
                    "user", CancellationToken.None, new FileDataStore("Prediction.HostedExample"));
            }

            // Create the service.
            var service = new PredictionService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Prediction API Sample",
            });

            // Make a prediction.
            Console.WriteLine("Performing a prediction...");
            string text = "mucho bueno";
            Console.WriteLine("Enter a test to analyze [{0}]: ", text);
            string userInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(userInput))
            {
                text = userInput;
            }
            var input = new Input { InputValue = new Input.InputData { CsvInstance = new List<object> { text } } };
            Output result = await service.Hostedmodels.Predict(input, "sample.languageid").ExecuteAsync();
            Console.WriteLine("Language: " + result.OutputLabel);
        }
    }
}
