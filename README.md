# Samples of using the [Google APIs Client Library for .NET](https://github.com/google/google-api-dotnet-client)

## Instructions for the Google .NET Client API – Samples solution

### Browse Online

*   [Browse Source](http://code.google.com/p/google-api-dotnet-client/source/browse?repo=samples)

### 1. Checkout Instructions

**Prerequisites:** Install Visual Studio or [MonoDevelop](http://monodevelop.com/), and [Mercurial](http://www.mercurial-scm.org/).

```
cd [someDirectory]
hg clone https://code.google.com/p/google-api-dotnet-client.samples/ google-api-dotnet-client-samples
```

### 2. API access Instructions

**Important:** after checking out the project, and before compiling and running it, you need to:

*   [Create a project in Google APIs console](http://code.google.com/apis/console-help/#creatingdeletingprojects)
*   [Activate](http://code.google.com/apis/console-help/#activatingapis) the APIs you want to use
*   Enter your client credentials by executing enterCredentials.cmd (or running `chmod +x configure && ./configure` on Unix) You can find your credentials on the [Google APIs Console](https://code.google.com/apis/console/) API Access pane.

### Setup Project in Visual Studio

*   Open the GoogleApisSamples.sln with Visual Studio
*   Click on Build > Rebuild Solution
*   Pick a sample, and execute the .exe in _&lt;SampleDir&gt;\bin\Debug_

### Setup Project in MonoDevelop

*   Open the GoogleApisSamples_mono.sln with MonoDevelop
*   Click on Build > Rebuild All
*   Pick a sample, and execute the .exe in _&lt;SampleDir&gt;\bin\Debug_ using mono
