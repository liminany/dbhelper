<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Demo</title>
</head>
<body>
    <form id="frm" runat="server">

		<asp:Literal ID="result" runat="server"></asp:Literal><br />
		<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
		<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBox1"
			Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
		<asp:Button ID="btnAdd" runat="server" Text="Insert Data" OnClick="btnAdd_Click" />
		<asp:GridView ID="list" runat="server"></asp:GridView>

    </form>
</body>
</html>
