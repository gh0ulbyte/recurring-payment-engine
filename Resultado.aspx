<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Code/Resultado.aspx.cs" Inherits="DebitosAutomaticos.Resultado" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Resultado - Débitos Automáticos</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .header {
            text-align: center;
            margin-bottom: 30px;
            color: #333;
        }
        .success {
            color: #27ae60;
            text-align: center;
            padding: 20px;
            background-color: #d5f4e6;
            border: 1px solid #27ae60;
            border-radius: 4px;
            margin: 20px 0;
        }
        .error {
            color: #e74c3c;
            text-align: center;
            padding: 20px;
            background-color: #fdf2f2;
            border: 1px solid #f5c6cb;
            border-radius: 4px;
            margin: 20px 0;
        }
        .info {
            color: #3498db;
            text-align: center;
            padding: 20px;
            background-color: #ebf3fd;
            border: 1px solid #3498db;
            border-radius: 4px;
            margin: 20px 0;
        }
        .details {
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 4px;
            margin: 20px 0;
        }
        .details h3 {
            margin-top: 0;
            color: #333;
        }
        .details p {
            margin: 10px 0;
            color: #666;
        }
        .btn {
            display: inline-block;
            padding: 12px 24px;
            background-color: #3498db;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            margin: 10px 5px;
            transition: background-color 0.3s;
        }
        .btn:hover {
            background-color: #2980b9;
        }
        .btn-success {
            background-color: #27ae60;
        }
        .btn-success:hover {
            background-color: #229954;
        }
        .btn-danger {
            background-color: #e74c3c;
        }
        .btn-danger:hover {
            background-color: #c0392b;
        }
        .actions {
            text-align: center;
            margin-top: 30px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>Resultado del Débito Automático</h1>
                <p>Estado de su solicitud de débito automático</p>
            </div>
            
            <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="success">
                <h3>✅ Débito Automático Configurado Exitosamente</h3>
                <p>Su débito automático ha sido configurado correctamente.</p>
            </asp:Panel>

            <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error">
                <h3>❌ Error en la Configuración</h3>
                <p><asp:Literal ID="litErrorMessage" runat="server" /></p>
            </asp:Panel>

            <asp:Panel ID="pnlInfo" runat="server" Visible="false" CssClass="info">
                <h3>ℹ️ Información del Débito Automático</h3>
                <p><asp:Literal ID="litInfoMessage" runat="server" /></p>
            </asp:Panel>

            <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="details">
                <h3>Detalles del Débito Automático</h3>
                <p><strong>ID de Transacción:</strong> <asp:Literal ID="litTransaccionId" runat="server" /></p>
                <p><strong>Tipo de Débito:</strong> <asp:Literal ID="litTipoDebito" runat="server" /></p>
                <p><strong>ID de Débito:</strong> <asp:Literal ID="litDebitoId" runat="server" /></p>
                <p><strong>Estado:</strong> <asp:Literal ID="litEstado" runat="server" /></p>
                <p><strong>Fecha de Creación:</strong> <asp:Literal ID="litFechaCreacion" runat="server" /></p>
                <p><strong>Próximo Débito:</strong> <asp:Literal ID="litProximoDebito" runat="server" /></p>
            </asp:Panel>

            <div class="actions">
                <asp:Button ID="btnConsultar" runat="server" Text="Consultar Estado" CssClass="btn btn-success" OnClick="btnConsultar_Click" />
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar Débito" CssClass="btn btn-danger" OnClick="btnCancelar_Click" />
                <a href="Default.aspx" class="btn">Nuevo Débito Automático</a>
            </div>
        </div>
    </form>
</body>
</html> 