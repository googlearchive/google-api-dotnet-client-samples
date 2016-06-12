Option Strict On
Option Explicit On
Imports System
Imports System.IO
Imports System.Reflection
Imports Google.Apis.Http
Imports Google.Apis.Requests
Imports Google.Apis.Services
Imports Google.Apis.Util
Imports Google.Apis.YouTube.v3
Imports Google.Apis.YouTube.v3.Data
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Upload
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks

Module ResumeableUploadSample
    ' 
    ' A VB .NET sample for the YouTube Data API using ResumableUpload.
    ' Demonstrates:
    '      .UploadAsync(CancellationToken cancellationToken) - Initiate a ResumableUpload.
    '      .ResumeAsync(CancellationToken cancellationToken) - After interruption, resume an upload within the same execution of the application.
    '      .ResumeAsync(CancellationToken cancellationToken, Uri uploadUri) - Resume an upload in a subsequent execution of the application.
    ' 
    ' See https://developers.google.com/api-client-library/dotnet/guide/media_upload for more details regarding ResumableUpload.
    ' See https://developers.google.com/resources/api-libraries/documentation/youtube/v3/csharp/latest/ for YouTube Data API documentation.
    ' See https://developers.google.com/youtube/v3/getting-started for details of the underlying API calls used by the .NET Client Library.
    ' See https://github.com/google/google-api-dotnet-client/releases for .NET Client Library Release Notes.
    ' See https://www.nuget.org/packages/Google.Apis.YouTube.v3/ for information on the .NET Client Library NuGet package.
    '
    ' The following command was used in the NuGet Package Manager Console Window to install the .NET Client Library NuGet package:
    '   Install-Package Google.Apis.YouTube.v3     
    ' 
    ' Prepare for running the sample program:
    '   1. Click Tools / NuGet Package Manager / Manage NuGet Packages for Solution
    '   2. Click on Updates / nuget.org and install all update packages
    '   3. Insert your CLIENTID and CLIENTSECRET in the source code below.
    '   4. Change VIDEOFULLPATHFILENAME below to specify the test video file to be uploaded.
    '   
    ' 
    ''' <summary>
    ''' <para>ClientId and ClientSecret are found in your client_secret_*****.apps.googleusercontent.com.json file downloaded from</para>
    ''' <para>the Google Developers Console (https://console.developers.google.com).</para>
    ''' <para>This sample shows the ClientID and ClientSecret in the source code.</para>
    ''' <para>Other samples in the sample library show how to read the Client Secrets from the client_secret_*****.apps.googleusercontent.com.json file.</para>
    ''' </summary>
    Const CLIENTID As String = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.apps.googleusercontent.com"
    ''' <summary>
    ''' ClientId and ClientSecret are found in your client_secret_*****.apps.googleusercontent.com.json file.
    ''' </summary>
    Const CLIENTSECRET As String = "xxxxxxxxxxxxxxxxxxxxxxxx"
    ''' <summary>
    ''' Fullpath filename of file to be uploaded
    ''' </summary>
    Private VIDEOFULLPATHFILENAME As String = "C:\Users\YourUsername\Videos\Video\Video.mp4"
    ''' <summary>
    ''' Maximum attempts to resume upload after a disruption.
    ''' </summary>
    Private Const MAX_RESUME_RETRIES = 3
    ''' <summary>
    ''' Cancellation Token
    ''' </summary>
    Private UploadCancellationToken As System.Threading.CancellationToken
    ''' <summary>
    ''' VideosResource.InsertMediaUpload Class object holds the metadata for the video to be uploaded.
    ''' </summary>
    Private WithEvents VideoInsertRequest As VideosResource.InsertMediaUpload = Nothing
    ''' <summary>
    '''  Used to compute percent complete
    ''' </summary>
    Private VideoFileLength As Long = 0
    ''' <summary>
    ''' Used to compare whole percent progress values
    ''' </summary>
    Private ProgressPercent As Integer = 0
    ''' <summary>
    ''' Used to count resume attempts and to terminate do loop after success
    ''' </summary>
    Private ResumeRetriesCount As Integer

    Sub Main()
        ExecuteUploadProcess().Wait()
    End Sub

    ''' <summary>
    ''' Execute the upload of one file to YouTube.
    ''' </summary>
    Private Async Function ExecuteUploadProcess() As Task
        Const HTTP_CLIENT_TIMEOUT_MINUTES = 2
        Dim OAUth2Credential As UserCredential = Nothing
        Dim YouTube As YouTubeService = Nothing
        Dim OKtoContinue As Boolean = True
        '
        ' Get OAuth2 Credential
        '
        OAUth2Credential = Await GetCredential()
        If OAUth2Credential IsNot Nothing AndAlso OAUth2Credential.Token IsNot Nothing Then
            Console.WriteLine(String.Format("Token Issued: {0}", OAUth2Credential.Token.Issued))
        Else
            OKtoContinue = False
        End If
        If OKtoContinue Then
            Try
                YouTube = New YouTubeService(New BaseClientService.Initializer() With
                    {
                    .HttpClientInitializer = OAUth2Credential,
                    .GZipEnabled = True,
                    .ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
                     })
                '
                ' Default Timeout is 1 minute 40 seconds. 
                ' The following changes it to 2 minutes. 
                YouTube.HttpClient.Timeout = TimeSpan.FromMinutes(HTTP_CLIENT_TIMEOUT_MINUTES)
            Catch ex As Exception
                Console.WriteLine(String.Format("YouTubeService error: {0}", ex.Message))
                OKtoContinue = False
            End Try
            If OKtoContinue Then
                Dim VideoResource As Video = CreateVideoResource()
                Using VideoFileStream As FileStream = New FileStream(VIDEOFULLPATHFILENAME, FileMode.Open, FileAccess.Read, FileShare.Read)
                    VideoInsertRequest = YouTube.Videos.Insert(VideoResource, "snippet,status,recordingDetails", VideoFileStream, "video/*")
                    Await PerformUpload()
                    VideoFileStream.Close()
                End Using
            End If
        End If
    End Function
    ''' <summary>
    ''' Perform the upload and resume if upload fails
    ''' </summary>
    Private Async Function PerformUpload() As Task
        Const MinimumChunkSize As Integer = 256 * 1024          ' .25MB (256K)
        Const HalfMBChunkSize As Integer = MinimumChunkSize * 2 ' .5MB (512K)
        VideoInsertRequest.ChunkSize = HalfMBChunkSize          ' .5MB
        AddHandler VideoInsertRequest.ResponseReceived, AddressOf VideoInsertRequest_ResponseReceived
        AddHandler VideoInsertRequest.ProgressChanged, AddressOf VideoInsertRequest_ProgressChanged
        ' The following line is only required if your application will support resuming upon a program restart
        AddHandler VideoInsertRequest.UploadSessionData, AddressOf VideoInsertRequest_UploadSessionData
        UploadCancellationToken = New System.Threading.CancellationToken
        ResumeRetriesCount = 0
        ProgressPercent = 0
        VideoFileLength = New FileInfo(VIDEOFULLPATHFILENAME).Length
        Try
            ' Check if previous upload program was terminated and, if so, prompt to resume prior upload
            Dim uploadUri As Uri = GetSessionRestartUri()
            If uploadUri = Nothing Then
                Await VideoInsertRequest.UploadAsync(UploadCancellationToken)
            Else
                Console.WriteLine("Restarting prior upload session.")
                Await VideoInsertRequest.ResumeAsync(UploadCancellationToken, uploadUri)
            End If
        Catch ex As Exception
            ' Error handling is done in VideoInsertRequest_ProgressChanged() Event.
            ' Exception messages displayed here seem to be redundant.
            Console.WriteLine(ex.Message)
        End Try
        Dim WasResumeAsyncLaunched As Boolean = False
        Do
            Thread.Sleep(3000) ' Allow time for ProgressChanged() Event to execute and to allow time for a server or network issue to resolve
            WasResumeAsyncLaunched = False
            If IsResumeable() Then
                ' Try a ResumeAsync
                ResumeRetriesCount += 1
                WasResumeAsyncLaunched = True
                Try
                    Await VideoInsertRequest.ResumeAsync(UploadCancellationToken)
                Catch ex As Exception
                    ' Error handling is done in VideoInsertRequest_ProgressChanged() Event.
                    ' Exception messages displayed here seem to be redundant.
                    Console.WriteLine(ex.Message)
                End Try
            End If
        Loop While WasResumeAsyncLaunched AndAlso IsResumeable()
    End Function
    ''' <summary>
    '''  Create the Video object containing the metadata about the video to be uploaded
    ''' </summary>
    ''' <returns>Video</returns>
    Private Function CreateVideoResource() As Video
        CreateVideoResource = New Video
        With CreateVideoResource
            .Snippet = New VideoSnippet With
                       {.Title = "Test Title",
                        .Description = "Test Description",
                        .Tags = {"keyword1", "keyword2", "keyword3"},
                        .CategoryId = "25"}   ' News & Politics
            .Status = New Google.Apis.YouTube.v3.Data.VideoStatus With
                      {.Embeddable = True,
                       .PrivacyStatus = "private"} ' PrivacyStatus must be all lowercase
            .RecordingDetails = New VideoRecordingDetails With
                                {.Location = New GeoPoint With
                                    {.Latitude = 41.327464, .Longitude = -72.194555, .Altitude = 10},
                                    .LocationDescription = "East Lyme, CT, U.S.A.",
                                    .RecordingDate = DateTime.Now.Date}
        End With
    End Function
    ''' <summary>
    ''' Get credential
    ''' </summary>
    ''' <returns>UserCredential</returns>
    Private Async Function GetCredential() As Task(Of UserCredential)
        '
        ' ClientId and ClientSecret are found in your client_secret_*****.apps.googleusercontent.com.json file downloaded from 
        ' the Google Developers Console (https://console.developers.google.com). 
        ' This sample shows the ClientID and ClientSecret in the source code. 
        ' Other samples in the sample library show how to read the Client Secrets from the client_secret_*****.apps.googleusercontent.com.json file. 
        '
        ' If a prior credential data store ("Google.Apis.Auth.YouTube") is not available, will prompt for usename & password.
        ' If a prior credential data store ("Google.Apis.Auth.YouTube") is found, will, if necessary, refresh the credentials.
        Dim uc As UserCredential = Nothing
        Try
            uc = Await GoogleWebAuthorizationBroker.AuthorizeAsync(
                 New ClientSecrets With {.ClientId = CLIENTID, _
                                        .ClientSecret = CLIENTSECRET},
                                    {YouTubeService.Scope.Youtube},
                                    "user",
                                    CancellationToken.None,
                                    New Google.Apis.Util.Store.FileDataStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Google.Apis.Auth.YouTube")))
        Catch ex As Exception
            Console.WriteLine(String.Format("Google Authorization: {0}", ex.Message))
        End Try
        Return uc
    End Function
    ''' <summary>
    ''' <para>If an UploadUri was saved in a previous execution of this program and the full path filename saved with the UploadUri matches</para>
    ''' <para>the full path filename of the file currently being uploaded, prompt the user to approve resuming the upload.</para>
    ''' <para>Otherwise, return null indicating that the upload should be started from the beginning.</para>
    ''' </summary>
    ''' <remarks>
    '''  This method is only required if your application will support resuming upon a program restart
    ''' </remarks>
    ''' <returns>UploadUri if found and if user approves resuming. Otherwise, null is returned.</returns>
    Private Function GetSessionRestartUri() As Uri
        If My.Settings.ResumeUri.Length > 0 AndAlso My.Settings.ResumeFilename = VIDEOFULLPATHFILENAME Then
            ' An UploadUri from a previous execution is present, ask if a resume should be attempted
            If MsgBox(String.Format("Resume previous upload?{0}{0}{1}", vbNewLine, VIDEOFULLPATHFILENAME), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Resume Upload") = MsgBoxResult.Yes Then
                Return New Uri(My.Settings.ResumeUri)
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function
    ''' <summary>
    ''' Determines if a .ResumeAsync() should be attempted.
    ''' </summary>
    ''' <returns>True means .ResumeAsync should be attempted.</returns>
    ''' <remarks>When Globals.ResumeRetries==int32.MaxValue, the upload has completed successfully and IsResumeable() will return false.</remarks>
    Private Function IsResumeable() As Boolean
        Return CBool(ResumeRetriesCount < MAX_RESUME_RETRIES AndAlso (Not UploadCancellationToken.IsCancellationRequested))
    End Function
    ''' <summary>
    ''' <para>The event handler called by the .NET Client Library upon initialization of the resumable session URI.</para>
    ''' <para>This event handler is not needed if your application will not support resuming after a program restart.</para>
    ''' <para>*</para>
    ''' <para>Called once for each file after the upload has been initialized and the resumeable session URI (UploadUri) is available.</para>
    ''' <para>The example is for a Windows Desktop application. Change the contents of this method to suit your tastes and platform.</para>
    ''' <para>*</para>
    ''' <para>It is strongly recommended that the full path filename be saved along with the UploadUri string value so that</para>
    ''' <para>upon program restart, the full path filename can be compared with the full path filename currently opened.</para>
    ''' <para>See GetSessionRestartUri() in this sample proggram.</para>
    ''' </summary>
    ''' <param name="UploadSessionData">ResumeableUploadSessionData Class object containing the resumable session URI (UploadUri)</param>
    Private Sub VideoInsertRequest_UploadSessionData(ByVal uploadSessionData As IUploadSessionData)
        ' Save UploadUri.AbsoluteUri and FullPath Filename values for use if program faults and we want to restart the program
        My.Settings.ResumeUri = uploadSessionData.UploadUri.AbsoluteUri
        My.Settings.ResumeFilename = VIDEOFULLPATHFILENAME
        ' Saved to a user.config file within a subdirectory of C:\Users\<yourusername>\AppData\Local
        My.Settings.Save()
    End Sub
    ''' <summary>
    ''' The event handler called by the .NET Client Library to indicate the current status of the upload.
    ''' </summary>
    ''' <param name="uploadStatusInfo">IUploadProgress object passed by upload process</param>
    Private Sub VideoInsertRequest_ProgressChanged(ByVal uploadStatusInfo As IUploadProgress)
        Select Case uploadStatusInfo.Status

            Case UploadStatus.Completed
                ResumeRetriesCount = Int32.MaxValue ' Ensure that Do..While loop terminates
                Console.WriteLine("Status: Completed")

            Case UploadStatus.Starting
                Console.WriteLine("Status: Starting")

            Case UploadStatus.NotStarted
                Console.WriteLine("Status: Not Started")

            Case UploadStatus.Uploading
                Dim sngPercent As Single = 0.0
                ResumeRetriesCount = 0 ' Successfully transmitted something so reset resume retries counter
                If VideoFileLength > 0 Then
                    sngPercent = CSng((100.0 * uploadStatusInfo.BytesSent) / VideoFileLength)
                End If
                '
                ' Display only whole percents
                '
                Dim p As Integer = CInt(sngPercent)
                If p > ProgressPercent Then
                    ' Only report whole percent progress
                    Console.WriteLine(String.Format("Status: Uploading {0:N0}% ({1:N})", p, uploadStatusInfo.BytesSent))
                    ProgressPercent = p
                End If

            Case UploadStatus.Failed
                Console.WriteLine(String.Format("Upload Failed: {0}", uploadStatusInfo.Exception.Message))
        End Select
    End Sub
    ''' <summary>
    ''' The event handler called by the .NET Client Library to provide the Video object containing the Id of the uploaded video.
    ''' </summary>
    ''' <param name="videoResource">Video object passed by upload process</param>
    Private Sub VideoInsertRequest_ResponseReceived(ByVal videoResource As Video)
        Console.WriteLine(String.Format("Video ID={0} uploaded.", videoResource.Id))
        ' Set to empty strings to indciate that upload completed.
        My.Settings.ResumeUri = ""
        My.Settings.ResumeFilename = ""
        ' Saved to a user.config file within a subdirectory of C:\Users\<yourusername>\AppData\Local
        My.Settings.Save()
    End Sub

End Module
