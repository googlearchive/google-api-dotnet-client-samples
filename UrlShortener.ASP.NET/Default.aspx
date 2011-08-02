<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UrlShortener.ASP.NET._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <h1>Urlshortener ASP.NET sample</h1>
    <form id="mainForm" runat="server">
    <p>
        Please enter a shortened or a long url into the textbox, and press the button next to it.
    </p>
    <div>
        <asp:Label ID="Label1" runat="server" Font-Bold="True" Text="Input:"></asp:Label> &nbsp; &nbsp;
        <asp:TextBox ID="input" runat="server" AutoPostBack="True" 
            ontextchanged="input_TextChanged"></asp:TextBox>
        <asp:Button ID="action" runat="server" Text="Shorten!" onclick="action_Click" />
    
    </div>
    <p>
        <asp:Label ID="Label2" runat="server" Font-Bold="True" Text="Output:"></asp:Label> &nbsp; &nbsp;
        <asp:Label ID="output" runat="server"></asp:Label>
    </p>
    </form>
    <p>
        &copy; 2011 Google Inc</p>
</body>
</html>
