## Instructions for the Google .NET Client API – Translate.WP.TranslateText

### Browse Online

*   [Browse Source](http://code.google.com/p/google-api-dotnet-client/source/browse/?repo=samples#hg%2FTranslate.WP.TranslateText), or main file [MainPage.xaml.cs](http://code.google.com/p/google-api-dotnet-client/source/browse/Translate.WP.TranslateText/MainPage.xaml.cs?repo=samples)

### 1. Checkout Instructions

**Prerequisites:** Install Visual Studio, and [Mercurial](http://www.mercurial-scm.org/).

```
cd [someDirectory] 
hg clone https://code.google.com/p/google-api-dotnet-client.samples/ google-api-dotnet-client-samples
```

### 2. API access Instructions

**Important:** after checking out the project, and before compiling and running it, you need to:

*   [Create a project in Google APIs console](https://developers.google.com/console/help/?csw=1#creatingdeletingprojects)
*   [Activate](https://developers.google.com/console/help/?csw=1#activatingapis) the Translate API for your project

### Setup Project in Visual Studio

*   Open the GoogleApisSamples.sln with Visual Studio
*   Click on Build > Rebuild Solution
*   Find your API Key which is generated on the [Google APIs Console](https://console.developers.google.com/project) API Access pane.
*   Edit MainPage.xaml.cs and replace the value of the API_KEY constant with your API Key
*   Launch the app in the Windows Phone emulator
