'Copyright 2011 Google Inc
'
'Licensed under the Apache License, Version 2.0(the "License");
'you may not use this file except in compliance with the License.
'You may obtain a copy of the License at
'
'    http://www.apache.org/licenses/LICENSE-2.0
'
'Unless required by applicable law or agreed to in writing, software
'distributed under the License is distributed on an "AS IS" BASIS,
'WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'See the License for the specific language governing permissions and
'limitations under the License.
'

Imports Google.Apis.Discovery.v1
Imports Google.Apis.Discovery.v1.Data

''' <summary>
''' This example uses the discovery API to list all APIs in the discovery repository.
''' http://code.google.com/apis/discovery/v1/using.html
''' </summary>
Class Program
    Shared Sub Main()
        Console.WriteLine("Discovery API Sample")
        Console.WriteLine("====================")

        ' Create the service.
        Dim service = New DiscoveryService()
        RunSample(service)

        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub

    Private Shared Sub RunSample(ByVal service As DiscoveryService)
        ' Run the request.
        Console.WriteLine("Executing a list request...")
        Dim result = service.Apis.List().Execute()

        ' Display the results.
        If result.Items IsNot Nothing Then
            For Each api As DirectoryList.ItemsData In result.Items
                Console.WriteLine(api.Id & " - " & api.Title)
            Next
        End If
    End Sub
End Class