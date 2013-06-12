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
using System.Threading.Tasks;

using DotNetOpenAuth.OAuth2;

using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Util;

namespace Books.ListMyLibrary
{
    /// <summary>
    /// Sample which demonstrates how to use the Books API.
    /// Lists all volumes in the the users library, and retrieves more detailed information about the first volume.
    /// https://code.google.com/apis/books/docs/v1/getting_started.html
    /// </summary>
    internal class Program
    {
        private static readonly string Scope = BooksService.Scopes.Books.GetStringValue();

        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Books API: List MyLibrary");

            // Register the authenticator.
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                };
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication);

            // Create the service.
            var service = new BooksService(new BaseClientService.Initializer()
                {
                    Authenticator = auth
                });

            ListLibrary(service);
            Console.ReadLine();
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.books";
            const string KEY = "=UwuqAtRaqe-3daV";

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

        private static void ListLibrary(BooksService service)
        {
            CommandLine.WriteAction("Listing Bookshelves... (using async execution)");
            // execute async
            var task = service.Mylibrary.Bookshelves.List().ExecuteAsync();

            // on success display my library's volumes
            CommandLine.WriteLine();
            task.ContinueWith(async t => await DisplayVolumes(service, t.Result),
                TaskContinuationOptions.OnlyOnRanToCompletion);

            // on failure print the error
            task.ContinueWith(t =>
                {
                    CommandLine.Write("Error occurred on executing async operation");
                    if (t.IsCanceled)
                    {
                        CommandLine.Write("Task was canceled");
                    }
                    if (t.Exception != null)
                    {
                        CommandLine.Write("exception occurred. Exception is " + t.Exception.Message);
                    }
                }, TaskContinuationOptions.NotOnRanToCompletion);
        }

        private static async Task DisplayVolumes(BooksService service, Bookshelves bookshelves)
        {
            if (bookshelves.Items == null)
            {
                CommandLine.WriteError("No bookshelves found!");
                return;
            }

            foreach (Bookshelf item in bookshelves.Items)
            {
                CommandLine.WriteResult(item.Title, item.VolumeCount + " volumes");

                // List all volumes in this bookshelf.
                if (item.VolumeCount > 0)
                {
                    CommandLine.WriteAction("Query volumes... (Execute Async)");
                    var request = service.Mylibrary.Bookshelves.Volumes.List(item.Id.ToString());
                    Volumes inBookshelf = await request.ExecuteAsync();
                    if (inBookshelf.Items == null)
                    {
                        continue;
                    }

                    foreach (Volume volume in inBookshelf.Items)
                    {
                        CommandLine.WriteResult(
                            "-- " + volume.VolumeInfo.Title, volume.VolumeInfo.Description ??
                            "no description");
                    }
                }
            }
        }
    }
}
