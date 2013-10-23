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

namespace Prediction.Simple
{
    /// <summary>
    /// Sample for the prediction API.
    /// This sample trains a simple model, and makes a prediction on the user input.
    /// 
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
                    "user", CancellationToken.None, new FileDataStore("PredictionAPIStore"));
            }

            // Create the service.
            var service = new PredictionService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Prediction API Sample",
                });

            // Train the service with the existing bucket data.
            string id = "<My Bucket>/language_id.txt";

            // Instructions on enabling G-Storage: http://code.google.com/apis/storage/docs/signup.html.
            Console.WriteLine("Performing training of the service ...");
            Console.WriteLine("Bucket " + id);
            Training training = new Training { Id = id };
            training = await service.Training.Insert(training).ExecuteAsync();

            // Wait until the training is complete.
            while (training.TrainingStatus == "RUNNING")
            {
                Console.WriteLine("...");
                await TaskEx.Delay(1000);
                training = await service.Training.Get(id).ExecuteAsync();
            }
            Console.WriteLine();
            Console.WriteLine("Training complete!");
            Console.WriteLine();

            // Make a prediction.
            Console.WriteLine("Performing a prediction...");

            string text = "mucho bueno";
            Console.Write("Enter test to analyze [{0}]: ", text);
            var userInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(userInput))
            {
                text = userInput;
            }
            var input = new Input { InputValue = new Input.InputData { CsvInstance = new List<object> { text } } };
            Output result = await service.Training.Predict(input, id).ExecuteAsync();
            Console.WriteLine("Language: {0}", result.OutputLabel);
        }
    }
}
