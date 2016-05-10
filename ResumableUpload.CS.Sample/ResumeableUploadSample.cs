using System;
using System.IO;
using System.Reflection;
using Google.Apis.Http;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Upload;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace ResumableUpload.CS.Sample
{
    class ResumeableUploadSample
    {
        /// <summary>
        /// A C# .NET sample for the YouTube Data API using ResumableUpload.
        /// 
        /// See https://developers.google.com/api-client-library/dotnet/guide/media_upload for more details regarding ResumableUpload.
        /// See https://developers.google.com/resources/api-libraries/documentation/youtube/v3/csharp/latest/ for YouTube Data API documentation.
        /// See https://developers.google.com/youtube/v3/getting-started for details of the underlying API calls used by the .NET Client Library.
        /// See https://github.com/google/google-api-dotnet-client/releases for .NET Client Library Release Notes.
        /// See https://www.nuget.org/packages/Google.Apis.YouTube.v3/ for information on the .NET Client Library NuGet package.
        /// 
        /// Prepare for running the sample program:
        ///   1. Add Reference to System.Net.Http
        ///   2. In NuGet Package Manager Console, enter:
        ///       Install-Package Google.Apis.YouTube.v3 
        ///   3. Click Tools / NuGet Package Manager / Manage NuGet Packages for Solution
        ///   4. Click on Updates / nuget.org and install all update packages
        ///   5. Insert your ClientId and ClientSecret in the source code below.
        ///   6. Change strFilePath below to specify the test video file to be uploaded.
        /// </summary>
        const int MAX_RESUME_RETRIES = 3;
        static VideosResource.InsertMediaUpload objVideosInsertRequest = null;
        static System.Threading.CancellationToken objCancellationToken;

        static void Main(string[] args)
        {
            ExecuteUploadProcess().Wait();
        }

        /// <summary>
        /// Execute the upload of one file to YouTube.
        /// </summary>
        private static async Task ExecuteUploadProcess()
        {
            const int intMinimumChunkSize = 256 * 1024; // .25MB (256K)
            const int intHalfMBChunkSize = intMinimumChunkSize * 2; // .5MB (512K)
            const int HTTP_CLIENT_TIMEOUT_MINUTES = 2;
            UserCredential objOAUth2Credential = null;
            YouTubeService objYouTubeService = null;
            bool bOKtoContinue = false;
            try
             {
            //
            // ClientId and ClientSecret are found in your client_secret_*****.apps.googleusercontent.com.json file downloaded from 
            // the Google Developers Console ( https://console.developers.google.com). 
            // This sample shows the ClientID and ClientSecret in the source code. 
            // Other samples in the sample library show how to read the Client Secrets from the client_secret_*****.apps.googleusercontent.com.json file. 
            //
            objOAUth2Credential = await GoogleWebAuthorizationBroker.AuthorizeAsync (
                 new ClientSecrets {ClientId = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.apps.googleusercontent.com",
                                    ClientSecret = "xxxxxxxxxxxxxxxxxxxxxxxx"},
                                    new[] {YouTubeService.Scope.Youtube},
                                    "user",
                                    CancellationToken.None,
                                    new Google.Apis.Util.Store.FileDataStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Google.Apis.Auth.YouTube")));
            if (objOAUth2Credential != null )
                {
                if (objOAUth2Credential.Token != null)
                 {
                    Console.WriteLine("Token Issued: {0}", objOAUth2Credential.Token.Issued);
                    }
                }
            bOKtoContinue =true;
            }
        catch (Exception ex)
            {
            Console.WriteLine("Google Authorization: {0}", ex.Message);
            }

            if (bOKtoContinue)
                {
                //
                // Upload a file
                //
                try
                    {
                    objYouTubeService = new YouTubeService(new BaseClientService.Initializer
                        {
                        HttpClientInitializer = objOAUth2Credential,
                        GZipEnabled = true,
                        ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
                        });
                    //
                    // Default Timeout is 1 minute 40 seconds. 
                    // The following changes it to 2 minutes. 
                    objYouTubeService.HttpClient.Timeout = TimeSpan.FromMinutes(HTTP_CLIENT_TIMEOUT_MINUTES);
                    bOKtoContinue = true;
                    }
                catch (Exception ex)
                    {
                    Console.WriteLine(String.Format("YouTubeService error: {0}", ex.Message));
                    }

                if (bOKtoContinue)
                    {
                    Video objVideo  = new Video();
                    VideoSnippet vs = new VideoSnippet
                    {
                        Title = "Test Title",
                        Description = "Test Description",
                        Tags = new string[] { "keyword1", "keyword2", "keyword3" },
                        CategoryId = "25" // News & Politics
                    };
                    objVideo.Snippet = vs;
                    objVideo.Status = new VideoStatus {Embeddable = true,PrivacyStatus = "private"};
                    objVideo.RecordingDetails = new VideoRecordingDetails 
                                        {Location = new GeoPoint 
                                            {Latitude = 41.327464, Longitude = -72.194555, Altitude = 10},
                                            LocationDescription = "East Lyme, CT, U.S.A.",
                                            RecordingDate=DateTime.Now.Date};
                    string strFilePath = "C:\\Users\\YourUserName\\Videos\\Video\\YourVideo.mp4";
                    Globals.FileLength = new FileInfo(strFilePath).Length;
                    Globals.ProgressPercent = 0;
                    using (FileStream objFileStream = new FileStream(strFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                        objVideosInsertRequest = objYouTubeService.Videos.Insert(objVideo, "snippet,status,recordingDetails", objFileStream, "video/*");
                        objVideosInsertRequest.ChunkSize = intHalfMBChunkSize;  // .5MB
                        objVideosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived; // Add event handler
                        objVideosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;   // Add event handler
                        objCancellationToken = new System.Threading.CancellationToken();
                        Globals.ResumeRetries = 0;
                        try
                        {
                            await objVideosInsertRequest.UploadAsync(objCancellationToken);
                        }
                        catch
                        {
                            // Error handling is done in videosInsertRequest_ProgressChanged() Event
                        }
                        bool bResumeAsyncWasLaunched = false;
                        do
                        {
                            Thread.Sleep(3000); // Allow time for ProgressChanged() Event to execute and to allow time for a server or network issue to resolve
                            bResumeAsyncWasLaunched = false;
                            if (IsResumeable())
                            {
                                // Try a ResumeAsync
                                Globals.ResumeRetries += 1;
                                bResumeAsyncWasLaunched = true;
                                try
                                {
                                   await objVideosInsertRequest.ResumeAsync(objCancellationToken);
                                }
                                catch
                                {
                                    // Error handling is done in videosInsertRequest_ProgressChanged() Event
                                }
                            }
                        } while (bResumeAsyncWasLaunched && IsResumeable());
                        objFileStream.Close();
                    }
                }
            }
        }
        /// <summary>Determines if a .ResumeAsync should be attempted.</summary>
        /// <returns>True then .ResumeAsync should be attempted.</returns>
        /// <remarks>When Globals.ResumeRetries==int32.MaxValue, the upload has completed successfully and IsResumeable() will return false.</remarks>
        private static bool IsResumeable()
        {
            return ((Globals.ResumeRetries < MAX_RESUME_RETRIES) && (!objCancellationToken.IsCancellationRequested));
        }
        /// <summary>
        /// The event handler called by the .NET Client Library to indicate the current status of the upload.
        /// </summary>
        /// <param name="objUploadStatus"></param>
        static void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress objUploadStatus)
        {
            switch (objUploadStatus.Status)
            {
                case UploadStatus.Completed:
                    Globals.ResumeRetries = Int32.MaxValue; // Ensure that Do..While loop terminates
                    Console.WriteLine("Status: Completed");
                    break;
                case UploadStatus.Starting:
                    Console.WriteLine("Status: Starting");
                    break;
                case UploadStatus.NotStarted:
                    Console.WriteLine("Status: Not Started");
                    break;
                case UploadStatus.Uploading:
                    float sngPercent = 0;
                    Globals.ResumeRetries = 0; // Successfully transmitted something so reset resume retries counter
                    if (Globals.FileLength > 0)
                        {
                        sngPercent = (float)((100.0 * objUploadStatus.BytesSent) / Globals.FileLength);
                        }
                    //
                    // Display only whole percents
                    //
                    int p = (int)sngPercent;
                    if (p > Globals.ProgressPercent)
                        {
                        string strMessage =String.Format( "Status: Uploading {0:##0}% ({1:###,###,###,###,###,###,###,###})", p, objUploadStatus.BytesSent);
                        Console.WriteLine(strMessage);
                        Globals.ProgressPercent = (int)sngPercent;
                        }
                    break;
                case UploadStatus.Failed:
                    Console.WriteLine(string.Format("Upload Failed: {0}", objUploadStatus.Exception.Message));
                    break;
            }
        }
        /// <summary>
        /// The event handler called by the .NET Client Library to provide the Video object containing the Id of the uploaded video.
        /// </summary>
        /// <param name="objVideo"></param>
        static void videosInsertRequest_ResponseReceived(Video objVideo)
        {
            Console.WriteLine(string.Format("Video ID={0} uploaded.", objVideo.Id));
        }

    }
    /// <summary>
    /// Holds global variables required to computer percent complete and count retries.
    /// </summary>
    public static class Globals
    {
        // Globally Addressable Variables
        public static long FileLength { get; set; }   // Used to compute percent complete
        public static int ProgressPercent { get; set; } // Used to compare whole percent progress values
        public static int ResumeRetries{ get; set; } // Used to count resume attempts and to terminate do loop after success
    }
}
