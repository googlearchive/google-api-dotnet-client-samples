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
using System.Threading;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Discovery;
using Google.Apis.Prediction.v1_3;
using Google.Apis.Prediction.v1_3.Data;
using Google.Apis.Samples.Helper;
using Google.Apis.Util;
using Prediction.HostedExample;

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
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Prediction API");

            CommandLine.WriteLine();

            // Register the authenticator.
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                };

            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication);

            // Create the service.
            var service = new PredictionService(new BaseClientService.Initializer()
                {
                    Authenticator = auth
                });

            RunPrediction(service);
            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.prediction";
            const string KEY = "AF41sdBra7ufra)VD:@#A#a++=3e";
            string scope = PredictionService.Scopes.Prediction.GetStringValue();

            // Check if there is a cached refresh token available.
            IAuthorizationState state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY);
            if (state != null)
            {
                try
                {
                    client.RefreshToken(state);
                    return state; // Yes - we are done.
                }
                catch (DotNetOpenAuth.Messaging.ProtocolException ex)
                {
                    CommandLine.WriteError("Using existing refresh token failed: " + ex.Message);
                }
            }

            // Retrieve the authorization from the user.
            state = AuthorizationMgr.RequestNativeAuthorization(client, scope);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
            return state;
        }

        private static void RunPrediction(PredictionService service)
        {
            // Make a prediction.
            CommandLine.WriteAction("Performing a prediction...");
            string text = "mucho bueno";
            CommandLine.RequestUserInput("Text to analyze", ref text);

            var input = new Input { InputValue = new Input.InputData { CsvInstance = new List<string> { text } } };
            Output result = service.Hostedmodels.Predict(input, "sample.languageid").Fetch();
            CommandLine.WriteResult("Language", result.OutputLabel);
        }
    }
}
