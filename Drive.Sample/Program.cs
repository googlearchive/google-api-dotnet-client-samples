/*
Copyright 2013 Google Inc

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

using Google;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Logging;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util;

namespace Drive.Sample
{
    /// <summary>
    /// A sample for the Drive API. This samples demonstrates resumable media upload and media download.
    /// See https://developers.google.com/drive/ for more details regarding the Drive API.
    /// </summary>
    class Program
    {
        static Program()
        {
            // initialize the log instance
            ApplicationContext.RegisterLogger(new Log4NetLogger());
            Logger = ApplicationContext.Logger.ForType<ResumableUpload<Program>>();
        }

        #region Consts

        private const int KB = 0x400;
        private const int DownloadChunkSize = 256 * KB;

        // CHANGE THIS with full path to the file you want to upload
        private const string UploadFileName = @"FILE_TO_UPLOAD_HERE";

        // CHANGE THIS with a download directory
        private const string DownloadDirectoryName = @"DIRECTORY_TO_SAVE_THE_DOWNLOADED_MEDIA_HERE";

        // CHANGE THIS if you upload a file type other than a jpg
        private const string ContentType = @"image/jpeg";

        #endregion

        /// <summary>The logger instance.</summary>
        private static readonly ILogger Logger;

        /// <summary>The Drive API scopes.</summary>
        private static readonly string[] Scopes = new[] { 
            DriveService.Scopes.DriveFile.GetStringValue(), DriveService.Scopes.Drive.GetStringValue() };

        /// <summary>
        /// The file which was uploaded. We will use its download Url to download it using our media downloader object.
        /// </summary>
        private static File uploadedFile;

        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Drive API");

            // Register the authenticator.
            FullClientCredentials credentials = PromptingClientCredentials.EnsureFullClientCredentials();
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                {
                    ClientIdentifier = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                };
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);

            // Create the service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                Authenticator = auth,
                ApplicationName = "Drive API Sample",
            });

            UploadFileAsync(service).ContinueWith(t =>
                {
                    // uploaded succeeded
                    Console.WriteLine("\"{0}\" was uploaded successfully", uploadedFile.Title);
                    DownloadFile(service, uploadedFile.DownloadUrl);
                    DeleteFile(service, uploadedFile);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            CommandLine.PressAnyKeyToExit();
        }

        /// <summary>Uploads file asynchronously.</summary>
        private static Task<IUploadProgress> UploadFileAsync(DriveService service)
        {
            var title = UploadFileName;
            if (title.LastIndexOf('\\') != -1)
            {
                title = title.Substring(title.LastIndexOf('\\') + 1);
            }
            var uploadStream = new System.IO.FileStream(UploadFileName, System.IO.FileMode.Open,
                System.IO.FileAccess.Read);
            var insert = service.Files.Insert(new File
                {
                    Title = title,
                }, uploadStream, ContentType);

            insert.ChunkSize = FilesResource.InsertMediaUpload.MinimumChunkSize * 2;
            insert.ProgressChanged += Upload_ProgressChanged;
            insert.ResponseReceived += Upload_ResponseReceived;

            var task = insert.UploadAsync();

            task.ContinueWith(t =>
            {
                // NotOnRanToCompletion - this code will be called if the upload fails
                Console.WriteLine("Upload Filed. " + t.Exception);
            }, TaskContinuationOptions.NotOnRanToCompletion);
            task.ContinueWith(t =>
            {
                Logger.Debug("Closing the stream");
                uploadStream.Dispose();
                Logger.Debug("The stream was closed");
            });

            return task;
        }

        /// <summary>Downloads the media from the given URL.</summary>
        private static void DownloadFile(DriveService service, string url)
        {
            var downloader = new MediaDownloader(service);
            downloader.ChunkSize = DownloadChunkSize;
            // add a delegate for the progress changed event for writing to console on changes
            downloader.ProgressChanged += Download_ProgressChanged;

            // figure out the right file type base on UploadFileName extension
            var lastDot = UploadFileName.LastIndexOf('.');
            var fileName = DownloadDirectoryName + @"\Download" +
                (lastDot != -1 ? "." + UploadFileName.Substring(lastDot + 1) : "");
            using (var fileStream = new System.IO.FileStream(fileName,
                System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var progress = downloader.Download(url, fileStream);
                if (progress.Status == DownloadStatus.Completed)
                {
                    Console.WriteLine(fileName + " was downloaded successfully");
                }
                else
                {
                    Console.WriteLine("Download {0} was interpreted in the middle. Only {1} were downloaded. ",
                        fileName, progress.BytesDownloaded);
                }
            }
        }

        /// <summary>Deletes the given file from drive (not the file system).</summary>
        private static void DeleteFile(DriveService service, File file)
        {
            CommandLine.WriteLine("Deleting file '{0}'...", file.Id);
            service.Files.Delete(file.Id).Execute();
            CommandLine.WriteLine("File was deleted successfully");
        }

        #region Progress and Response changes

        static void Download_ProgressChanged(IDownloadProgress progress)
        {
            Console.WriteLine(progress.Status + " " + progress.BytesDownloaded);
        }

        static void Upload_ProgressChanged(IUploadProgress progress)
        {
            Console.WriteLine(progress.Status + " " + progress.BytesSent);
        }

        static void Upload_ResponseReceived(File file)
        {
            uploadedFile = file;
        }

        #endregion

        private static IAuthorizationState GetAuthorization(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.drive";
            const string KEY = "y},drdzf11x9;87";

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
            state = AuthorizationMgr.RequestNativeAuthorization(client, Scopes);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
            return state;
        }
    }
}
