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
Imports System.IO
Imports System.Threading

Imports Google.Apis.Calendar.v3
Imports Google.Apis.Calendar.v3.Data
Imports Google.Apis.Calendar.v3.EventsResource
Imports Google.Apis.Services
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Util.Store

''' <summary>
''' An sample for the Calendar API which displays a list of calendars and events in the first calendar.
''' https://developers.google.com/google-apps/calendar/
''' </summary>
Module Program

    '' Calendar scopes which is initialized on the main method.
    Dim scopes As IList(Of String) = New List(Of String)()

    '' Calendar service.
    Dim service As CalendarService

    Sub Main()
        ' Add the calendar specific scope to the scopes list.
        scopes.Add(CalendarService.Scope.Calendar)

        ' Display the header and initialize the sample.
        Console.WriteLine("Google.Apis.Calendar.v3 Sample")
        Console.WriteLine("==============================")

        Dim credential As UserCredential
        Using stream As New FileStream("client_secrets.json", FileMode.Open, FileAccess.Read)
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets, scopes, "user", CancellationToken.None,
                    New FileDataStore("Calendar.VB.Sample")).Result
        End Using

        ' Create the calendar service using an initializer instance
        Dim initializer As New BaseClientService.Initializer()
        initializer.HttpClientInitializer = credential
        initializer.ApplicationName = "VB.NET Calendar Sample"
        service = New CalendarService(initializer)

        ' Fetch the list of calendar list
        Dim list As IList(Of CalendarListEntry) = service.CalendarList.List().Execute().Items()

        ' Display all calendars
        DisplayList(list)
        For Each calendar As Data.CalendarListEntry In list
            ' Display calendar's events
            DisplayFirstCalendarEvents(calendar)
        Next

        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub

    ''' <summary>Displays all calendars.</summary>
    Private Sub DisplayList(list As IList(Of CalendarListEntry))
        Console.WriteLine("Lists of calendars:")
        For Each item As CalendarListEntry In list
            Console.WriteLine(item.Summary & ". Location: " & item.Location & ", TimeZone: " & item.TimeZone)
        Next
    End Sub

    ''' <summary>Displays the calendar's events.</summary>
    Private Sub DisplayFirstCalendarEvents(list As CalendarListEntry)
        Console.WriteLine(Environment.NewLine & "Maximum 5 first events from {0}:", list.Summary)
        Dim requeust As ListRequest = service.Events.List(list.Id)
        ' Set MaxResults and TimeMin with sample values
        requeust.MaxResults = 5
        requeust.TimeMin = New DateTime(2013, 10, 1, 20, 0, 0)
        ' Fetch the list of events
        For Each calendarEvent As Data.Event In requeust.Execute().Items
            Dim startDate As String = "Unspecified"
            If (Not calendarEvent.Start Is Nothing) Then
                If (Not calendarEvent.Start.Date Is Nothing) Then
                    startDate = calendarEvent.Start.Date.ToString()
                End If
            End If

            Console.WriteLine(calendarEvent.Summary & ". Start at: " & startDate)
        Next
    End Sub

End Module
