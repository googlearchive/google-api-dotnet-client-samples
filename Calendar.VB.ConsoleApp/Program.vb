'Copyright 2013 Google Inc

'Licensed under the Apache License, Version 2.0(the "License");
'you may not use this file except in compliance with the License.
'You may obtain a copy of the License at

'    http://www.apache.org/licenses/LICENSE-2.0

'Unless required by applicable law or agreed to in writing, software
'distributed under the License is distributed on an "AS IS" BASIS,
'WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'See the License for the specific language governing permissions and
'limitations under the License.

Imports System.Collections.Generic

Imports DotNetOpenAuth.OAuth2

Imports Google.Apis.Authentication.OAuth2
Imports Google.Apis.Authentication.OAuth2.DotNetOpenAuth
Imports Google.Apis.Calendar.v3
Imports Google.Apis.Calendar.v3.Data
Imports Google.Apis.Calendar.v3.EventsResource
Imports Google.Apis.Samples.Helper
Imports Google.Apis.Services
Imports Google.Apis.Util

''' <summary>
''' An sample for the Calendar API which displays a list of calendars and events in the first calendar.
''' https://developers.google.com/google-apps/calendar/
''' </summary>
Module Program

    '' Calendar scopes which is initialized on the main method
    Dim scopes As IList(Of String) = New List(Of String)()

    '' Calendar service
    Dim service As CalendarService

    Sub Main()
        ' Add the calendar specific scope to the scopes list
        scopes.Add(CalendarService.Scopes.Calendar.GetStringValue())

        ' Display the header and initialize the sample
        CommandLine.EnableExceptionHandling()
        CommandLine.DisplayGoogleSampleHeader("Google.Api.Calendar.v3 Sample")

        ' Create the authenticator
        Dim credentials As FullClientCredentials = PromptingClientCredentials.EnsureFullClientCredentials()
        Dim provider = New NativeApplicationClient(GoogleAuthenticationServer.Description)
        provider.ClientIdentifier = credentials.ClientId
        provider.ClientSecret = credentials.ClientSecret
        Dim auth As New OAuth2Authenticator(Of NativeApplicationClient)(provider, AddressOf GetAuthorization)

        ' Create the calendar service using an initializer instance
        Dim initializer As New BaseClientService.Initializer()
        initializer.Authenticator = auth
        service = New CalendarService(initializer)

        ' Fetch the list of calendar list
        Dim list As IList(Of CalendarListEntry) = service.CalendarList.List().Execute().Items()

        ' Display all calendars
        DisplayList(list)
        For Each calendar As Data.CalendarListEntry In list
            ' Display calendar's events
            DisplayFirstCalendarEvents(calendar)
        Next

        CommandLine.PressAnyKeyToExit()
    End Sub

    Function GetAuthorization(client As NativeApplicationClient) As IAuthorizationState
        ' You should use a more secure way of storing the key here as
        ' .NET applications can be disassembled using a reflection tool.
        Const STORAGE As String = "google.samples.dotnet.calendar"
        Const KEY As String = "s0mekey"

        ' Check if there is a cached refresh token available.
        Dim state As IAuthorizationState = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY)
        If Not state Is Nothing Then
            Try
                client.RefreshToken(state)
                Return state ' we are done
            Catch ex As DotNetOpenAuth.Messaging.ProtocolException
                CommandLine.WriteError("Using an existing refresh token failed: " + ex.Message)
                CommandLine.WriteLine()
            End Try
        End If

        ' Retrieve the authorization from the user
        state = AuthorizationMgr.RequestNativeAuthorization(client, scopes.ToArray())
        AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state)
        Return state
    End Function

    ''' <summary>Displays all calendars.</summary>
    Private Sub DisplayList(list As IList(Of CalendarListEntry))
        CommandLine.WriteLine("Lists of calendars:")
        For Each item As CalendarListEntry In list
            CommandLine.WriteResult(item.Summary, "Location: " & item.Location & ", TimeZone: " & item.TimeZone)
        Next
    End Sub

    ''' <summary>Displays the calendar's events.</summary>
    Private Sub DisplayFirstCalendarEvents(list As CalendarListEntry)
        CommandLine.WriteLine(Environment.NewLine & "Maximum 5 first events from {0}:", list.Summary)
        Dim requeust As ListRequest = service.Events.List(list.Id)
        ' Set MaxResults and TimeMin with sample values
        requeust.MaxResults = 5
        requeust.TimeMin = "2012-01-01T00:00:00-00:00"
        ' Fetch the list of events
        For Each calendarEvent As Data.Event In requeust.Execute().Items
            Dim startDate As String = "Unspecified"
            If (Not calendarEvent.Start Is Nothing) Then
                If (Not calendarEvent.Start.Date Is Nothing) Then
                    startDate = calendarEvent.Start.Date.ToString()
                End If
            End If

            CommandLine.WriteResult(calendarEvent.Summary, "Start at: " & startDate)
        Next
    End Sub

End Module
