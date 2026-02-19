<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Code/TestLocal.aspx.cs" Inherits="DebitosAutomaticos.TestLocal" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Prueba Local - Débitos Automáticos</title>
    <meta charset="utf-8" />
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { text-align: center; margin-bottom: 30px; color: #333; }
        .form-group { margin-bottom: 20px; }
        .form-group label { display: block; margin-bottom: 5px; font-weight: bold; }
        .form-group input, .form-group select { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; }
        .btn { background-color: #3498db; color: white; padding: 12px 24px; border: none; border-radius: 4px; cursor: pointer; font-size: 16px; margin: 5px; }
        .btn:hover { background-color: #2980b9; }
        .result { margin-top: 20px; padding: 15px; border-radius: 4px; background-color: #f8f9fa; border: 1px solid #dee2e6; }
        .log { background-color: #f8f9fa; border: 1px solid #dee2e6; padding: 15px; border-radius: 4px; font-family: monospace; font-size: 12px; max-height: 300px; overflow-y: auto; white-space: pre-wrap; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>🧪 Prueba Local - Débitos Automáticos</h1>
                <p>Prueba el flujo sin base de datos</p>
            </div>
            
            <div class="form-group">
                <label for="ddlTipoDebito">Tipo de Débito:</label>
                <asp:DropDownList ID="ddlTipoDebito" runat="server">
                    <asp:ListItem Text="Suscripción" Value="suscripcion" />
                    <asp:ListItem Text="Adhesión" Value="adhesion" />
                    <asp:ListItem Text="Pago + Recurrencia" Value="pago_y_recurrencia" />
                </asp:DropDownList>
            </div>

            <div class="form-group">
                <label for="txtToken">Token:</label>
                <asp:TextBox ID="txtToken" runat="server" placeholder="TOKEN_001, TOKEN_002, TOKEN_003 o cualquier valor"></asp:TextBox>
            </div>

            <div class="form-group">
                <label for="txtTransaccionId">ID de Transacción:</label>
                <asp:TextBox ID="txtTransaccionId" runat="server" placeholder="1, 2, 3 o cualquier valor"></asp:TextBox>
            </div>

            <div style="text-align: center; margin: 30px 0;">
                <asp:Button ID="btnProbar" runat="server" Text="🚀 Probar Flujo" CssClass="btn" OnClick="btnProbar_Click" />
                <asp:Button ID="btnLimpiar" runat="server" Text="🧹 Limpiar" CssClass="btn" OnClick="btnLimpiar_Click" />
            </div>

            <asp:Panel ID="pnlResultado" runat="server" Visible="false" CssClass="result">
                <h3>📋 Resultado:</h3>
                <asp:Literal ID="litResultado" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlLog" runat="server" Visible="false">
                <h3>📝 Log:</h3>
                <div class="log">
                    <asp:Literal ID="litLog" runat="server" />
                </div>
            </asp:Panel>

            <div style="margin-top: 30px; padding: 20px; background-color: #e8f4f8; border-radius: 4px;">
                <h3>💡 Datos de Prueba Disponibles:</h3>
                <ul>
                    <li><strong>Tokens:</strong> TOKEN_001, TOKEN_002, TOKEN_003</li>
                    <li><strong>IDs:</strong> 1, 2, 3</li>
                    <li><strong>Usuarios:</strong> Juan Pérez, María García, Carlos López</li>
                    <li><strong>Montos:</strong> $1500, $2500, $3200</li>
                </ul>
            </div>
        </div>
    </form>
</body>
</html> 