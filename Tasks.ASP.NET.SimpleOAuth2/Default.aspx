<%@ Page Language="C#" 
    EnableSessionState="true" 
    AutoEventWireup="true" 
    CodeBehind="Default.aspx.cs" 
    Async="true"
    Inherits="Tasks.ASP.NET.SimpleOAuth2._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <h1>ASP.NET Tasks API &ndash; OAuth2 Sample</h1>
    <form id="mainForm" runat="server">
    Click the button below to authorize this application/list all TaskLists and Tasks.
    <br/><br/>
    <div>
        &nbsp; &nbsp; <asp:Button ID="listButton" runat="server" 
            Text="List Tasks!" onclick="listButton_Click" />
    </div>
    <br/>
    <asp:PlaceHolder ID="lists" runat="server"></asp:PlaceHolder><br/>
    <asp:Label ID="output" runat="server"></asp:Label>
    </form>
    <p>
        <i>&copy; 2011 Google Inc</i>
    </p>
</body>
</html>
