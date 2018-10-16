## Instructions for the Google .NET Client API – DfaReporting.Sample

### Browse Online

*   [Browse Source](http://code.google.com/p/google-api-dotnet-client/source/browse/?repo=samples#hg%2FDfaReporting.Sample) , or main file [Program.cs](http://code.google.com/p/google-api-dotnet-client/source/browse/DfaReporting.Sample/Program.cs?repo=samples)

### 1. Checkout Instructions

**Prerequisites:** Install Visual Studio, and [Mercurial](http://www.mercurial-scm.org/).

```
cd [someDirectory]
hg clone https://code.google.com/p/google-api-dotnet-client.samples/ google-api-dotnet-client-samples
```

### 2. API access Instructions

**Important:** after checking out the project, compiling and running it, you need to do the following:

*   Visit the [Google APIs console](https://code.google.com/apis/console/)
*   If this is your first time, click "Create project..."
*   Otherwise, click on the drop down under the "Google APIs" logo at the top left, and click "Create..." under "Other projects"
*   Click on "API Access", and then on "Create an OAuth 2.0 Client ID...".
*   Enter a product name and click "Next".
*   Select "Installed application" and click "Create client ID".
*   In the newly created "Client ID for installed applications", copy the client ID and client secrets into the AdSenseSample.cs file.
*   [Activate](https://code.google.com/apis/console/?api=dfareporting) the DFA Reporting API for your project.

### 3\. Set up Project in Visual Studio

*   Open the GoogleApisSamples.sln with Visual Studio
*   Click on Build > Rebuild Solution
*   Execute the .exe in _DfaReporting.Sample\bin\Debug_
