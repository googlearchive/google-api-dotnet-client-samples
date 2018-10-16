## Instructions for the Google .NET Client API – Blogger.Sample for Windows 8.1 and Windows Phone 8.1

### Browse Source

*   [Blogger.Sample](https://code.google.com/p/google-api-dotnet-client/source/browse?repo=samples#hg%2FBlogger.Sample)

### 1. Checkout Instructions

**Prerequisites:** Install Visual Studio, and [Mercurial](http://www.mercurial-scm.org/).

```
cd _[someDirectory]_
hg clone https://code.google.com/p/google-api-dotnet-client.samples/ google-api-dotnet-client-samples
```

### 2. API Access Instructions

**Important:** after checking out the project, you need to do the following:

*   [Create a project](https://developers.google.com/console/help/?csw=1#creatingdeletingprojects) in the Google APIs console
*   [Activate](https://developers.google.com/console/help/?csw=1#activatingapis) the Blogger API for your project
*   Update the client_id and client_secret in [client_secrets.json](https://code.google.com/p/google-api-dotnet-client/source/browse/Blogger.Sample/Blogger.Sample.Shared/Assets/client_secrets.json?repo=samples)

### Set Up Project in Visual Studio

1.  Open the GoogleApisSamples.sln with Visual Studio
2.  Build the solution
3.  Launch the Windows Phone or Windows app in the emulator
