using System;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Configuration;

namespace DebitosAutomaticos
{
    public partial class _Default : System.Web.UI.Page
    {
    private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            LogDebug("Iniciando procesamiento de débito automático");

        
            string token = Request.QueryString["token"];
            string tipoDebito = Request.QueryString["tipo"]; 
            string transaccionId = Request.QueryString["transaccionId"];

            LogDebug(String.Format("Token recibido: {0}", token));
            LogDebug(String.Format("Tipo de débito: {0}", tipoDebito));
            LogDebug(String.Format("ID de transacción: {0}", transaccionId));

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(tipoDebito) || string.IsNullOrEmpty(transaccionId))
            {
                LogError("Parámetros requeridos faltantes");
                ShowError("Parámetros requeridos faltantes: token, tipo y transaccionId");
                return;
            }

            LogDebug("Llamando a DebitoAutomaticoService.ProcesarDebito");
            
            try
            {
                
                string resultado = DebitoAutomaticoService.ProcesarDebito(token, tipoDebito, transaccionId);
                LogDebug(String.Format("Resultado del procesamiento: {0}", resultado));
                
                
                Response.Redirect("Resultado.aspx?resultado=" + HttpUtility.UrlEncode(resultado));
            }
            catch (Exception debitoEx)
            {
                LogError(String.Format("Error en ProcesarDebito: {0}", debitoEx.Message));
                LogError(String.Format("StackTrace: {0}", debitoEx.StackTrace));
                throw;
            }
        }
        catch (Exception ex)
        {
            LogError(String.Format("Error general: {0}", ex.Message));
            LogError(String.Format("StackTrace: {0}", ex.StackTrace));
            ShowError("Error al procesar el débito automático: " + ex.Message);
        }
    }

    private void ShowError(string message)
    {
        LogError(String.Format("Mostrando error: {0}", message));
        Response.Write("<div class='error'><h3>Error</h3><p>" + Server.HtmlEncode(message) + "</p></div>");
    }

    private void LogDebug(string message)
    {
        try
        {
            string logPath = Server.MapPath("~/App_Data/debug.log");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logMessage = String.Format("[{0}] {1}\r\n", timestamp, message);
            
            string directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.AppendAllText(logPath, logMessage);
        }
        catch (Exception ex)
        {
            // Si falla el logging principal, intentar un log alternativo
            try
            {
                string altLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "error.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = String.Format("[{0}] Error de logging: {1}\r\n", timestamp, ex.Message);
                File.AppendAllText(altLogPath, logMessage);
            }
            catch
            {
                // Si todo falla, no podemos hacer mucho más
            }
        }
    }

    private void LogError(string message)
    {
        try
        {
            string logPath = Server.MapPath("~/App_Data/error.log");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logMessage = String.Format("[{0}] ERROR: {1}\r\n", timestamp, message);
            
            string directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.AppendAllText(logPath, logMessage);
        }
        catch (Exception ex)
        {
            // Si falla el logging principal, intentar un log alternativo
            try
            {
                string altLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "error.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = String.Format("[{0}] Error de logging: {1}\r\n", timestamp, ex.Message);
                File.AppendAllText(altLogPath, logMessage);
            }
            catch
            {
                // Si todo falla, no podemos hacer mucho más
            }
        }
    }
    }
} 
