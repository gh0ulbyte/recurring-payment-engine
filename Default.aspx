<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Code/Default.aspx.cs" Inherits="DebitosAutomaticos._Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Débitos Automáticos -Proveedor</title>
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
        .loading {
            text-align: center;
            padding: 40px;
        }
        .spinner {
            border: 4px solid #f3f3f3;
            border-top: 4px solid #3498db;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 0 auto 20px;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
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
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>Procesando Débito Automático</h1>
                <p>Por favor espere mientras procesamos su solicitud...</p>
            </div>
            
            <div class="loading">
                <div class="spinner"></div>
                <p>Inicializando débito automático...</p>
            </div>
        </div>
    </form>
</body>
</html> 