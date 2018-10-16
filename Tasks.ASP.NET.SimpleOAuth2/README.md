# Google .NET Client API – Tasks.ASP.NET.SimpleOAuth2

## Instructions for the Google .NET Client API – Tasks.ASP.NET.SimpleOAuth2

### Browse Online

*   [Browse Source](http://code.google.com/p/google-api-dotnet-client/source/browse/?repo=samples#hg%2FTasks.ASP.NET.SimpleOAuth2), or main file [Default.aspx.cs](http://code.google.com/p/google-api-dotnet-client/source/browse/Tasks.ASP.NET.SimpleOAuth2/Default.aspx.cs?repo=samples)

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
*   In the newly created "Client ID for installed applications", click "Download JSON" on the right side. Replace the project's client_secrets.json with the file you just downloaded.
*   [Activate](https://code.google.com/apis/console/?api=tasks) the Tasks API for your project

### Setup Project in Visual Studio

*   Open the GoogleApisSamples.sln with Visual Studio
*   Click on Build > Rebuild Solution
*   Run the webpage using the builtin Visual Studio ASP.NET server (press F5)
