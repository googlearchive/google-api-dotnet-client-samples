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

using DotNetOpenAuth.OAuth2;

using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;
using Google.Apis.Util;

namespace UrlShortener.ListURLs
{
    /// <summary>
    /// URLShortener OAuth2 Sample
    /// 
    /// This sample uses OAuth2 to retrieve a list of all the URL's you have shortened so far.
    /// </summary>
    internal class Program
    {
        private static readonly string Scope = UrlshortenerService.Scopes.Urlshortener.GetStringValue();

        static void Main(string[] args)
        {
            // Initialize this sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("URLShortener -- List URLs");

            // Register the authenticator.
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret,
                };
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);

            // Create the service.
            var service = new UrlshortenerService(new BaseClientService.Initializer()
                {
                    Authenticator = auth,
                    ApplicationName = "UrlShortener API Sample",
                });

            // List all shortened URLs:
            CommandLine.WriteAction("Retrieving list of shortened urls...");

            int i = 0;
            string nextPageToken = null;
            do
            {
                // Create and execute the request.
                var request = service.Url.List();
                request.StartToken = nextPageToken;
                UrlHistory result = request.Execute();

                // List all items on this page.
                if (result.Items != null)
                {
                    foreach (Url item in result.Items)
                    {
                        CommandLine.WriteResult((++i) + ".) URL", item.Id + " -> " + item.LongUrl);
                    }
                }

                // Continue with the next page
                nextPageToken = result.NextPageToken;
            } while (!string.IsNullOrEmpty(nextPageToken));

            if (i == 0)
            {
                CommandLine.WriteAction("You don't have any shortened URLs! Visit http://goo.gl and create some.");
            }

            // ... and we are done.
            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthorization(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.urlshortener";
            const string KEY = "S7Uf8AsapUWrac798uga5U8e5azePhAf";

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
            state = AuthorizationMgr.RequestNativeAuthorization(client, Scope);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
            return state;
        }
    }
}
