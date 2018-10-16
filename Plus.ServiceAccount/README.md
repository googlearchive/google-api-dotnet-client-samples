## Instructions for the Google .NET Client API – Plus.ServiceAccount

### Browse Online

*   [Browse Source](http://code.google.com/p/google-api-dotnet-client/source/browse/?repo=samples#hg%2FPlus.ServiceAccount), or main file [Program.cs](http://code.google.com/p/google-api-dotnet-client/source/browse/Plus.ServiceAccount/Program.cs?repo=samples)

### 1. Checkout Instructions

**Prerequisites:** Install Visual Studio, and [Mercurial](http://www.mercurial-scm.org/).

```
cd [someDirectory]
hg clone https://code.google.com/p/google-api-dotnet-client.samples/ google-api-dotnet-client-samples
```

### 2. API access Instructions

**Important:** after checking out the project, and before compiling and running it, you need to:

*   [Create a project in Google APIs console](http://code.google.com/apis/console-help/#creatingdeletingprojects)
*   [Activate](https://code.google.com/apis/console/?api=plus) the Google+ API
*   Replace [key.p12](http://code.google.com/p/google-api-dotnet-client/source/browse/Plus.ServiceAccount/key.p12?repo=samples) with the private key that is generated on the [Google APIs Console](https://code.google.com/apis/console/) API Access pane for your Service Account.

### Setup Project in Visual Studio

*   Open the GoogleApisSamples.sln with Visual Studio
*   Click on Build > Rebuild Solution
*   Execute the .exe in _Plus.ServiceAccount\bin\Debug_
