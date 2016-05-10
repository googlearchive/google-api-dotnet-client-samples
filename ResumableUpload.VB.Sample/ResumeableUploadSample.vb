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
    ''' <summary>
    ''' A VB .NET sample for the YouTube Data API using ResumableUpload.
    ''' 
    ''' See https://developers.google.com/api-client-library/dotnet/guide/media_upload for more details regarding ResumableUpload.
    ''' See https://developers.google.com/resources/api-libraries/documentation/youtube/v3/csharp/latest/ for YouTube Data API documentation.
    ''' See https://developers.google.com/youtube/v3/getting-started for details of the underlying API calls used by the .NET Client Library.
    ''' See https://github.com/google/google-api-dotnet-client/releases for .NET Client Library Release Notes.
    ''' See https://www.nuget.org/packages/Google.Apis.YouTube.v3/ for information on the .NET Client Library NuGet package.
    ''' 
    ''' Prepare for running the sample program:
    '''   1. Add Reference to System.Net.Http
    '''   2. In NuGet Package Manager Console, enter:
    '''       Install-Package Google.Apis.YouTube.v3 
    '''   3. Click Tools / NuGet Package Manager / Manage NuGet Packages for Solution
    '''   4. Click on Updates / nuget.org and install all update packages
    '''   5. Insert your ClientId and ClientSecret in the source code below.
    '''   6. Change strFilePath below to specify the test video file to be uploaded.
    ''' </summary>
    Private Const MAX_RESUME_RETRIES = 3
    Private objCancellationToken As System.Threading.CancellationToken
    Private WithEvents objVideosInsertRequest As VideosResource.InsertMediaUpload = Nothing
    Private lngFileLength As Long = 0 ' Used to compute percent complete
    Private intProgressPercent As Integer = 0 ' Used to compare whole percent progress values
    Private intResumeRetries As Integer ' Used to count resume attempts and to terminate do loop after success

    Sub Main()
        ExecuteUploadProcess().Wait()
    End Sub

    ''' <summary>
    ''' Execute the upload of one file to YouTube.
    ''' </summary>
    Private Async Function ExecuteUploadProcess() As Task
        Const intMinimumChunkSize As Integer = 256 * 1024  ' .25MB (256K)
        Const intHalfMBChunkSize As Integer = intMinimumChunkSize * 2   ' .5MB (512K)
        Const HTTP_CLIENT_TIMEOUT_MINUTES = 2
        Dim objOAUth2Credential As UserCredential = Nothing
        Dim objYouTubeService As YouTubeService = Nothing
        Dim bOKtoContinue As Boolean = False

        Try
            '
            ' ClientId and ClientSecret are found in your client_secret_*****.apps.googleusercontent.com.json file downloaded from 
            ' the Google Developers Console ( https://console.developers.google.com). 
            ' This sample shows the ClientID and ClientSecret in the source code. 
            ' Other samples in the sample library show how to read the Client Secrets from the client_secret_*****.apps.googleusercontent.com.json file. 
            '
            objOAUth2Credential = Await GoogleWebAuthorizationBroker.AuthorizeAsync(
                 New ClientSecrets With {.ClientId = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.apps.googleusercontent.com",
                                         .ClientSecret = "xxxxxxxxxxxxxxxxxxxxxxxx"},
                                    {YouTubeService.Scope.Youtube},
                                    "user",
                                    CancellationToken.None,
                                    New Google.Apis.Util.Store.FileDataStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Google.Apis.Auth.YouTube")))
            If objOAUth2Credential IsNot Nothing Then
                If objOAUth2Credential.Token IsNot Nothing Then
                    Console.WriteLine(String.Format("Token Issued: {0}", objOAUth2Credential.Token.Issued))
                End If
            End If
            bOKtoContinue = True
        Catch ex As Exception
            Console.WriteLine(String.Format("Google Authorization: {0}", ex.Message))
            Exit Function
        End Try
        '
        ' Upload a file
        '
        If bOKtoContinue Then
            Try
                objYouTubeService = New YouTubeService(New BaseClientService.Initializer() With {
                    .HttpClientInitializer = objOAUth2Credential,
                    .GZipEnabled = True,
                    .ApplicationName = Assembly.GetExecutingAssembly().GetName().Name})
                '
                ' Default Timeout is 1 minute 40 seconds. 
                ' The following changes it to 2 minutes. 
                objYouTubeService.HttpClient.Timeout = TimeSpan.FromMinutes(HTTP_CLIENT_TIMEOUT_MINUTES)
                bOKtoContinue = True
            Catch ex As Exception
                Console.WriteLine(String.Format("YouTubeService error: {0}", ex.Message))
            End Try
            Dim x As System.Collections.Generic.IList(Of String) = {"keyword1", "keyword2", "keyword3"}

            If bOKtoContinue Then
                Dim objVideo As Video = New Video()
                With objVideo
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
                Dim strFilePath As String = "C:\Users\YourUserName\Videos\Video\YourVideo.mp4"
                lngFileLength = New FileInfo(strFilePath).Length
                intProgressPercent = 0
                Using objFileStream As FileStream = New FileStream(strFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    objVideosInsertRequest = objYouTubeService.Videos.Insert(objVideo, "snippet,status,recordingDetails", objFileStream, "video/*")
                    objVideosInsertRequest.ChunkSize = intHalfMBChunkSize ' .5MB
                    AddHandler objVideosInsertRequest.ResponseReceived, AddressOf videosInsertRequest_ResponseReceived  ' Add event handler
                    AddHandler objVideosInsertRequest.ProgressChanged, AddressOf videosInsertRequest_ProgressChanged    ' Add event handler
                    objCancellationToken = New System.Threading.CancellationToken
                    intResumeRetries = 0
                    Try
                        Await objVideosInsertRequest.UploadAsync(objCancellationToken)
                    Catch
                        ' Error handling is done in videosInsertRequest_ProgressChanged() Event
                    End Try
                    Dim bResumeAsyncWasLaunched As Boolean = False
                    Do
                        Thread.Sleep(3000) ' Allow time for ProgressChanged() Event to execute and to allow time for a server or network issue to resolve
                        bResumeAsyncWasLaunched = False
                        If IsResumeable() Then
                            ' Try a ResumeAsync
                            intResumeRetries += 1
                            bResumeAsyncWasLaunched = True
                            Try
                                Await objVideosInsertRequest.ResumeAsync(objCancellationToken)
                            Catch
                                ' Error handling is done in videosInsertRequest_ProgressChanged() Event
                            End Try
                        End If
                    Loop While bResumeAsyncWasLaunched AndAlso IsResumeable()
                    objFileStream.Close()
                End Using
            End If
        End If
    End Function
    ''' <summary>
    ''' Determines if a .ResumeAsync should be attempted.
    ''' </summary>
    ''' <returns>True then .ResumeAsync should be attempted.</returns>
    ''' <remarks>When Globals.ResumeRetries==int32.MaxValue, the upload has completed successfully and IsResumeable() will return false.</remarks>
    Private Function IsResumeable() As Boolean
        Return CBool(intResumeRetries < MAX_RESUME_RETRIES AndAlso (Not objCancellationToken.IsCancellationRequested))
    End Function
    ''' <summary>
    ''' The event handler called by the .NET Client Library to indicate the current status of the upload.
    ''' </summary>
    ''' <param name="objUploadStatus"></param>
    Private Sub videosInsertRequest_ProgressChanged(objUploadStatus As Google.Apis.Upload.IUploadProgress)
        Select Case objUploadStatus.Status

            Case UploadStatus.Completed
                intResumeRetries = Int32.MaxValue ' Ensure that Do..While loop terminates
                Console.WriteLine("Status: Completed")

            Case UploadStatus.Starting
                Console.WriteLine("Status: Starting")

            Case UploadStatus.NotStarted
                Console.WriteLine("Status: Not Started")

            Case UploadStatus.Uploading
                Dim sngPercent As Single = 0.0
                intResumeRetries = 0 ' Successfully transmitted something so reset resume retries counter
                If lngFileLength > 0 Then
                    sngPercent = CSng((100.0 * objUploadStatus.BytesSent) / lngFileLength)
                End If
                '
                ' Display only whole percents
                '
                Dim p As Integer = CInt(sngPercent)
                If p > intProgressPercent Then
                    Dim strMessage As String = String.Format("Status: Uploading {0:##0}% ({1:###,###,###,###,###,###,###,###})", p, objUploadStatus.BytesSent)
                    Console.WriteLine(strMessage)
                    intProgressPercent = CInt(sngPercent)
                End If

            Case UploadStatus.Failed
                Console.WriteLine(String.Format("Upload Failed: {0}", objUploadStatus.Exception.Message))
        End Select
    End Sub
    ''' <summary>
    ''' The event handler called by the .NET Client Library to provide the Video object containing the Id of the uploaded video.
    ''' </summary>
    ''' <param name="objVideo"></param>
    Private Sub videosInsertRequest_ResponseReceived(objVideo As Video)
        Console.WriteLine(String.Format("Video ID={0} uploaded.", objVideo.Id))
    End Sub

End Module
