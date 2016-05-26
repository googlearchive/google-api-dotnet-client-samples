using System;
using System.Windows.Forms;
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
        // A C# .NET sample for the YouTube Data API using ResumableUpload.
        // Demonstrates:
        //      UploadAsync(CancellationToken cancellationToken) - Initiate a ResumableUpload.
        //      ResumeAsync(CancellationToken cancellationToken) - After interruption, resume an upload within the same execution of the application.
        // 
        // See https://developers.google.com/api-client-library/dotnet/guide/media_upload for more details regarding ResumableUpload.
        // See https://developers.google.com/resources/api-libraries/documentation/youtube/v3/csharp/latest/ for YouTube Data API documentation.
        // See https://developers.google.com/youtube/v3/getting-started for details of the underlying API calls used by the .NET Client Library.
        // See https://github.com/google/google-api-dotnet-client/releases for .NET Client Library Release Notes.
        // See https://www.nuget.org/packages/Google.Apis.YouTube.v3/ for information on the .NET Client Library NuGet package.
        //
        // The following command was used in the NuGet Package Manager Console Window to install the .NET Client Library NuGet package:
        //   Install-Package Google.Apis.YouTube.v3     
        // 
        // Prepare for running the sample program:
        //   1. Click Tools / NuGet Package Manager / Manage NuGet Packages for Solution
        //   2. Click on Updates / nuget.org and install all update packages
        //   3. Insert your CLIENTID and CLIENTSECRET in the source code below.
        //   4. Change VIDEOFULLPATHFILENAME below to specify the test video file to be uploaded.
        //      
        /// <summary>
        /// <para>ClientId and ClientSecret are found in your client_secret_*****.apps.googleusercontent.com.json file downloaded from</para>
        /// <para>the Google Developers Console (https://console.developers.google.com).</para>
        /// <para>This sample shows the ClientID and ClientSecret in the source code.</para>
        /// <para>Other samples in the sample library show how to read the Client Secrets from the client_secret_*****.apps.googleusercontent.com.json file.</para>
        /// </summary>
        const string CLIENTID = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.apps.googleusercontent.com";
        /// <summary>
        /// <para>ClientId and ClientSecret are found in your client_secret_*****.apps.googleusercontent.com.json file.</para>
        /// </summary>
        const string CLIENTSECRET = "xxxxxxxxxxxxxxxxxxxxxxxx";
        /// <summary>
        /// Fullpath filename of file to be uploaded
        /// </summary>
        static string VIDEOFULLPATHFILENAME = @"C:\Users\<Your Username>\Videos\Video\Video.mp4";
        /// <summary>
        /// Maximum attempts to resume upload after a disruption.
        /// </summary>
        const int MAX_RESUME_RETRIES = 3;
        /// <summary>
        /// Cancellation Token
        /// </summary>
        static System.Threading.CancellationToken UploadCancellationToken;
        /// <summary>
        /// VideosResource.InsertMediaUpload Class object holds the metadata for the video to be uploaded.
        /// </summary>
        static VideosResource.InsertMediaUpload VideoInsertRequest = null;
        /// <summary>
        ///  Used to compute percent complete
        /// </summary>
        static long VideoFileLength;
        /// <summary>
        /// Used to compare whole percent progress values
        // </summary>
        static int ProgressPercent;
        /// <summary>
        /// Used to count resume attempts and to terminate do loop after success
        /// </summary>
        static int ResumeRetriesCount;

        static void Main(string[] args)
        {
            ExecuteUploadProcess().Wait();
        }

        /// <summary>
        /// Execute the upload of one file to YouTube.
        /// </summary>
        private static async Task ExecuteUploadProcess()
        {
            const int HTTP_CLIENT_TIMEOUT_MINUTES = 2;
            UserCredential OAUth2Credential = null;
            YouTubeService YouTube = null;
            bool OKtoContinue = true;
            try
            {
                OAUth2Credential = await GetCredential();
                if (OAUth2Credential != null && OAUth2Credential.Token != null)
                {
                    Console.WriteLine("Token Issued: {0}", OAUth2Credential.Token.Issued);
                }
                else
                {
                    OKtoContinue = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Google Authorization: {0}", ex.Message);
                OKtoContinue = false;
            }
            if (OKtoContinue)
            {
                //
                // Upload a file
                //
                try
                {
                    YouTube = new YouTubeService(new BaseClientService.Initializer
                        {
                            HttpClientInitializer = OAUth2Credential,
                            GZipEnabled = true,
                            ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
                        });
                    //
                    // Default Timeout is 1 minute 40 seconds. 
                    // The following changes it to 2 minutes. 
                    YouTube.HttpClient.Timeout = TimeSpan.FromMinutes(HTTP_CLIENT_TIMEOUT_MINUTES);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("YouTubeService error: {0}", ex.Message));
                    OKtoContinue = false;
                }
                if (OKtoContinue)
                {
                    Video VideoResource = CreateVideoResource();
                    using (FileStream VideoFileStream = new FileStream(VIDEOFULLPATHFILENAME, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        VideoInsertRequest = YouTube.Videos.Insert(VideoResource, "snippet,status,recordingDetails", VideoFileStream, "video/*");
                        await PerformUpload();
                        VideoFileStream.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Perform the upload and resume if upload fails
        /// </summary>
        private static async Task PerformUpload()
        {
            const int MinimumChunkSize = 256 * 1024; 	      // .25MB (256K)
            const int HalfMBChunkSize = MinimumChunkSize * 2; // .5MB (512K)
            VideoInsertRequest.ChunkSize = HalfMBChunkSize;   // .5MB
            VideoInsertRequest.ResponseReceived += VideoInsertRequest_ResponseReceived;
            VideoInsertRequest.ProgressChanged += VideoInsertRequest_ProgressChanged;
            UploadCancellationToken = new System.Threading.CancellationToken();
            ResumeRetriesCount = 0;
            ProgressPercent = 0;
            VideoFileLength = new FileInfo(VIDEOFULLPATHFILENAME).Length;
            try
            {
                await VideoInsertRequest.UploadAsync(UploadCancellationToken);
            }
            catch (Exception ex)
            {
                // Error handling is done in VideoInsertRequest_ProgressChanged() Event.
                // Exception messages displayed here seem to be redundant.
                Console.WriteLine(ex.Message);
            }
            bool WasResumeAsyncLaunched = false;
            do
            {
                Thread.Sleep(3000);
                // Allow time for ProgressChanged() Event to execute and to allow time for a server or network issue to resolve
                WasResumeAsyncLaunched = false;
                if (IsResumeable())
                {
                    // Try a ResumeAsync
                    ResumeRetriesCount += 1;
                    WasResumeAsyncLaunched = true;
                    try
                    {
                        await VideoInsertRequest.ResumeAsync(UploadCancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // Error handling is done in VideoInsertRequest_ProgressChanged() Event.
                        // Exception messages displayed here seem to be redundant.
                        Console.WriteLine(ex.Message);
                    }
                }
            } while (WasResumeAsyncLaunched && IsResumeable());
        }

        /// <summary>
        ///  Create the Video Resource object containing the metadata for the video to be uploaded
        /// </summary>
        /// <returns>Video</returns>
        private static Video CreateVideoResource()
        {
            Video VideoResource = new Video();
            VideoSnippet vs = new VideoSnippet
            {
                Title = "Test Title",
                Description = "Test Description",
                Tags = new string[] { "keyword1", "keyword2", "keyword3" },
                CategoryId = "25" // News & Politics
            };
            VideoResource.Snippet = vs;
            VideoResource.Status = new VideoStatus { Embeddable = true, PrivacyStatus = "private" }; // PrivacyStatus must be all lowercase.
            VideoResource.RecordingDetails = new VideoRecordingDetails
                                {
                                    Location = new GeoPoint { Latitude = 41.327464, Longitude = -72.194555, Altitude = 10 },
                                    LocationDescription = "East Lyme, CT, U.S.A.",
                                    RecordingDate = DateTime.Now.Date
                                };
            return VideoResource;
        }
        /// <summary>
        /// Get credential
        /// </summary>
        /// <returns>UserCredential</returns>
        private static async Task<UserCredential> GetCredential()
        {
            //
            // ClientId and ClientSecret are found in your client_secret_*****.apps.googleusercontent.com.json file downloaded from 
            // the Google Developers Console (https://console.developers.google.com). 
            // This sample shows the ClientID and ClientSecret in the source code. 
            // Other samples in the sample library show how to read the Client Secrets from the client_secret_*****.apps.googleusercontent.com.json file. 
            //
            UserCredential uc = null;
            try {
                uc = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                         new ClientSecrets
                         {
                             ClientId = CLIENTID,
                             ClientSecret = CLIENTSECRET
                         },
                            new[] { YouTubeService.Scope.Youtube },
                            "user",
                            CancellationToken.None,
                            new Google.Apis.Util.Store.FileDataStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Google.Apis.Auth.YouTube")));

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Google Authorization: {0}", ex.Message));
            }
            return uc;
        }
        /// <summary>Determines if a .ResumeAsync should be attempted.</summary>
        /// <returns>True means .ResumeAsync() should be attempted.</returns>
        /// <remarks>When Globals.ResumeRetries==int32.MaxValue, the upload has completed successfully and IsResumeable() will return false.</remarks>
        private static bool IsResumeable()
        {
            return ((ResumeRetriesCount < MAX_RESUME_RETRIES) && (!UploadCancellationToken.IsCancellationRequested));
        }
        /// <summary>
        /// The event handler called by the .NET Client Library to indicate the current status of the upload.
        /// </summary>
        /// <param name="uploadStatusInfo">IUploadProgress object passed by upload process</param>
        static void VideoInsertRequest_ProgressChanged(IUploadProgress uploadStatusInfo)
        {
            switch (uploadStatusInfo.Status)
            {
                case UploadStatus.Completed:
                    ResumeRetriesCount = Int32.MaxValue; // Ensure that Do..While loop terminates
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
                    ResumeRetriesCount = 0; // Successfully transmitted something so reset resume retries counter
                    if (VideoFileLength > 0)
                    {
                        sngPercent = (float)((100.0 * uploadStatusInfo.BytesSent) / VideoFileLength);
                    }
                    //
                    // Display only whole percents
                    //
                    int p = (int)sngPercent;
                    if (p > ProgressPercent)
                    {
                        // Only report whole percent progress
                        string msg = String.Format("Status: Uploading {0:N0}% ({1:N})", p, uploadStatusInfo.BytesSent);
                        Console.WriteLine(msg);
                        ProgressPercent = p;
                    }
                    break;
                case UploadStatus.Failed:
                    Console.WriteLine(string.Format("Upload Failed: {0}", uploadStatusInfo.Exception.Message));
                    break;
            }
        }
        /// <summary>
        /// The event handler called by the .NET Client Library to provide the Video object containing the Id of the uploaded video.
        /// </summary>
        /// <param name="videoResource">Video object passed by upload process</param>
        static void VideoInsertRequest_ResponseReceived(Video videoResource)
        {
            Console.WriteLine(string.Format("Video ID={0} uploaded.", videoResource.Id));
        }
    }
}
