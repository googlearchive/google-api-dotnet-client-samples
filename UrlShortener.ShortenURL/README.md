## Instructions for the Google .NET Client API – UrlShortener.ShortenURL

### Browse Online

*   [Browse Source](http://code.google.com/p/google-api-dotnet-client/source/browse/?repo=samples#hg%2FUrlShortener.ShortenURL), or main file [Program.cs](http://code.google.com/p/google-api-dotnet-client/source/browse/UrlShortener.ShortenURL/Program.cs?repo=samples)

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
*   [Activate](https://code.google.com/apis/console/?api=urlshortener) the UrlShortener API for your project

### Setup Project in Visual Studio

*   Open the GoogleApisSamples.sln with Visual Studio
*   Click on Build > Rebuild Solution
*   Execute the .exe in _UrlShortener.ShortenURL\bin\Debug_
