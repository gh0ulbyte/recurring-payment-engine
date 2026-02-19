<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestDebitoAutomatico.aspx.cs" Inherits="DebitosAutomaticos.TestDebitoAutomatico" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test Débito Automático - PayperTIC</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f5f5f5;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
            background-color: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .header {
            text-align: center;
            color: #333;
            margin-bottom: 30px;
        }
        .form-group {
            margin-bottom: 15px;
        }
        label {
            display: block;
            margin-bottom: 5px;
            font-weight: bold;
            color: #555;
        }
        input[type="text"], input[type="email"], input[type="number"], select, textarea {
            width: 100%;
            padding: 8px;
            border: 1px solid #ddd;
            border-radius: 4px;
            box-sizing: border-box;
        }
        .btn {
            background-color: #007bff;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            margin-right: 10px;
            margin-bottom: 10px;
        }
        .btn:hover {
            background-color: #0056b3;
        }
        .btn-success {
            background-color: #28a745;
        }
        .btn-success:hover {
            background-color: #218838;
        }
        .btn-danger {
            background-color: #dc3545;
        }
        .btn-danger:hover {
            background-color: #c82333;
        }
        .btn-warning {
            background-color: #ffc107;
            color: #212529;
        }
        .btn-warning:hover {
            background-color: #e0a800;
        }
        .result {
            margin-top: 20px;
            padding: 15px;
            border-radius: 4px;
            white-space: pre-wrap;
            font-family: monospace;
            font-size: 12px;
        }
        .result.success {
            background-color: #d4edda;
            border: 1px solid #c3e6cb;
            color: #155724;
        }
        .result.error {
            background-color: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
        }
        .result.info {
            background-color: #d1ecf1;
            border: 1px solid #bee5eb;
            color: #0c5460;
        }
        .section {
            margin-bottom: 30px;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 4px;
        }
        .section h3 {
            margin-top: 0;
            color: #333;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>Test Débito Automático - PayperTIC</h1>
                <p>Prueba de funcionalidades de débito automático según documentación oficial</p>
            </div>

            <!-- Sección de Datos de Prueba -->
            <div class="section">
                <h3>Datos de Prueba</h3>
                <div class="form-group">
                    <label for="txtTransaccionId">ID de Transacción:</label>
                    <asp:TextBox ID="txtTransaccionId" runat="server" Text="1" />
                </div>
                <div class="form-group">
                    <label for="txtToken">Token:</label>
                    <asp:TextBox ID="txtToken" runat="server" Text="TOKEN_001" />
                </div>
                <div class="form-group">
                    <label for="txtTipoDebito">Tipo de Débito:</label>
                    <asp:DropDownList ID="ddlTipoDebito" runat="server">
                        <asp:ListItem Value="adhesion" Text="Adhesión" />
                        <asp:ListItem Value="suscripcion" Text="Suscripción" />
                        <asp:ListItem Value="pago_y_recurrencia" Text="Pago + Recurrencia" />
                    </asp:DropDownList>
                </div>
            </div>

            <!-- Sección de Operaciones -->
            <div class="section">
                <h3>Operaciones de Débito Automático</h3>
                <asp:Button ID="btnProcesarDebito" runat="server" Text="Procesar Débito" CssClass="btn btn-success" OnClick="btnProcesarDebito_Click" />
                <asp:Button ID="btnCrearAdhesion" runat="server" Text="Crear Adhesión" CssClass="btn" OnClick="btnCrearAdhesion_Click" />
                <asp:Button ID="btnCrearSuscripcion" runat="server" Text="Crear Suscripción" CssClass="btn" OnClick="btnCrearSuscripcion_Click" />
                <asp:Button ID="btnCrearCobroAdhesion" runat="server" Text="Crear Cobro a Adhesión" CssClass="btn" OnClick="btnCrearCobroAdhesion_Click" />
            </div>

            <!-- Sección de Consultas -->
            <div class="section">
                <h3>Consultas</h3>
                <div class="form-group">
                    <label for="txtIdConsulta">ID para Consulta:</label>
                    <asp:TextBox ID="txtIdConsulta" runat="server" placeholder="Ingrese ID de adhesión, suscripción o pago" />
                </div>
                <asp:Button ID="btnConsultarAdhesion" runat="server" Text="Consultar Adhesión" CssClass="btn" OnClick="btnConsultarAdhesion_Click" />
                <asp:Button ID="btnConsultarSuscripcion" runat="server" Text="Consultar Suscripción" CssClass="btn" OnClick="btnConsultarSuscripcion_Click" />
            </div>

            <!-- Sección de Cancelaciones -->
            <div class="section">
                <h3>Cancelaciones</h3>
                <div class="form-group">
                    <label for="txtIdCancelacion">ID para Cancelación:</label>
                    <asp:TextBox ID="txtIdCancelacion" runat="server" placeholder="Ingrese ID de adhesión, suscripción o pago" />
                </div>
                <asp:Button ID="btnCancelarAdhesion" runat="server" Text="Cancelar Adhesión" CssClass="btn btn-danger" OnClick="btnCancelarAdhesion_Click" />
                <asp:Button ID="btnCancelarSuscripcion" runat="server" Text="Cancelar Suscripción" CssClass="btn btn-danger" OnClick="btnCancelarSuscripcion_Click" />
                <asp:Button ID="btnCancelarPago" runat="server" Text="Cancelar Pago" CssClass="btn btn-warning" OnClick="btnCancelarPago_Click" />
            </div>

            <!-- Resultado -->
            <div class="form-group">
                <label>Resultado:</label>
                <asp:Label ID="lblResultado" runat="server" CssClass="result" />
            </div>
        </div>
    </form>
</body>
</html>
