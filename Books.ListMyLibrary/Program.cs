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
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Books.ListMyLibrary
{
    /// <summary>
    /// Sample which demonstrates how to use the Books API.
    /// Lists all volumes in the the users library, and retrieves more detailed information about the first volume.
    /// https://code.google.com/apis/books/docs/v1/getting_started.html
    /// </summary>
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Books API Sample: List MyLibrary");
            Console.WriteLine("================================");

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
                    new[] { BooksService.Scope.Books },
                    "user", CancellationToken.None, new FileDataStore("Books.ListMyLibrary"));
            }

            // Create the service.
            var service = new BooksService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Books API Sample",
                });

            await ListLibrary(service);
        }

        private async Task ListLibrary(BooksService service)
        {
            Console.WriteLine("Listing Bookshelves... (Execute ASYNC)");
            Console.WriteLine("======================================");

            // Execute async.
            var bookselve = await service.Mylibrary.Bookshelves.List().ExecuteAsync();

            // On success display my library's volumes.
            await DisplayVolumes(service, bookselve);
        }

        private async Task DisplayVolumes(BooksService service, Bookshelves bookshelves)
        {
            if (bookshelves.Items == null)
            {
                Console.WriteLine("No bookshelves found!");
                return;
            }

            foreach (Bookshelf item in bookshelves.Items)
            {
                Console.WriteLine(item.Title + "\t" +
                    (item.VolumeCount.HasValue ? item.VolumeCount + " volumes" : ""));

                // List all volumes in this bookshelf.
                if (item.VolumeCount > 0)
                {
                    Console.WriteLine("Query volumes... (Execute ASYNC)");
                    Console.WriteLine("================================");
                    var request = service.Mylibrary.Bookshelves.Volumes.List(item.Id.ToString());
                    Volumes inBookshelf = await request.ExecuteAsync();
                    if (inBookshelf.Items == null)
                    {
                        continue;
                    }
                    foreach (Volume volume in inBookshelf.Items)
                    {
                        Console.WriteLine("-- " + volume.VolumeInfo.Title + "\t" + volume.VolumeInfo.Description ??
                            "no description");
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
