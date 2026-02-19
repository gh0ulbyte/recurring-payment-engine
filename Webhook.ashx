<%@ WebHandler Language="C#" Class="DebitosAutomaticos.Webhook" %>

using System;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using System.Configuration;

namespace DebitosAutomaticos
{
    public class Webhook : IHttpHandler
    {
        private static readonly JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        private static string logPath;

        static Webhook()
        {
            try
            {
                string appDataPath = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "App_Data");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                logPath = Path.Combine(appDataPath, "webhook_debito.log");
                LogDebug("Webhook inicializado correctamente");
            }
            catch (Exception ex)
            {
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "webhook_debito.log");
                try
                {
                    LogDebug("Webhook inicializado con ruta alternativa");
                }
                catch { }
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                LogDebug("Webhook recibido - Iniciando procesamiento");

                // Leer el cuerpo de la petición
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = reader.ReadToEnd();
                }

                LogDebug(String.Format("Cuerpo de la petición: {0}", requestBody));

                if (string.IsNullOrEmpty(requestBody))
                {
                    LogError("Cuerpo de la petición vacío");
                    context.Response.StatusCode = 400;
                    context.Response.Write("Bad Request - Empty body");
                    return;
                }

                // Parsear el JSON
                var webhookData = jsonSerializer.Deserialize<dynamic>(requestBody);
                LogDebug("JSON parseado correctamente");

                // Procesar según el tipo de notificación según documentación Proveedor
                string tipoNotificacion = webhookData.ContainsKey("type") ? webhookData["type"] : "";
                string id = webhookData.ContainsKey("id") ? webhookData["id"] : "";
                string status = webhookData.ContainsKey("status") ? webhookData["status"] : "";

                LogDebug(String.Format("Tipo de notificación: {0}, ID: {1}, Status: {2}", tipoNotificacion, id, status));

                switch (tipoNotificacion.ToLower())
                {
                    case "adhesion":
                        ProcesarAdhesion(id, status, webhookData);
                        break;

                    case "debit":
                        ProcesarDebito(id, status, webhookData);
                        break;

                    case "subscription":
                        ProcesarSuscripcion(id, status, webhookData);
                        break;

                    default:
                        LogDebug(String.Format("Tipo de notificación no manejado: {0}", tipoNotificacion));
                        // Procesar como notificación genérica
                        ProcesarNotificacionGenerica(id, tipoNotificacion, status, webhookData);
                        break;
                }

                // Responder con éxito
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"status\":\"success\"}");

                LogDebug("Webhook procesado exitosamente");
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error en webhook: {0}", ex.Message));
                LogError(String.Format("StackTrace: {0}", ex.StackTrace));

                context.Response.StatusCode = 500;
                context.Response.Write("Internal Server Error");
            }
        }

        private void ProcesarAdhesion(string adhesionId, string status, dynamic webhookData)
        {
            try
            {
                LogDebug(String.Format("Procesando adhesión: {0}, Status: {1}", adhesionId, status));

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            IF EXISTS (SELECT 1 FROM ProveedorDA WHERE Adhesion_ID = @AdhesionId)
                                UPDATE ProveedorDA 
                                SET Estado = @Estado, 
                                    Fecha_Actualizacion = @FechaActualizacion,
                                    Webhook_Data = @WebhookData
                                WHERE Adhesion_ID = @AdhesionId
                            ELSE
                                INSERT INTO Proveedor (Adhesion_ID, Estado, Fecha_Creacion, Webhook_Data)
                                VALUES (@AdhesionId, @Estado, @FechaActualizacion, @WebhookData)";

                        command.Parameters.AddWithValue("@AdhesionId", adhesionId);
                        command.Parameters.AddWithValue("@Estado", status);
                        command.Parameters.AddWithValue("@FechaActualizacion", DateTime.Now);
                        command.Parameters.AddWithValue("@WebhookData", jsonSerializer.Serialize(webhookData));

                        int rowsAffected = command.ExecuteNonQuery();
                        LogDebug(String.Format("Filas afectadas en adhesión: {0}", rowsAffected));
                    }
                }

                LogDebug(String.Format("Adhesión procesada: {0}", adhesionId));
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al procesar adhesión: {0}", ex.Message));
                throw;
            }
        }

        private void ProcesarDebito(string pagoId, string status, dynamic webhookData)
        {
            try
            {
                LogDebug(String.Format("Procesando débito: {0}, Status: {1}", pagoId, status));

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Extraer información del webhook
                    decimal monto = 0;
                    if (webhookData.ContainsKey("final_amount"))
                        monto = Convert.ToDecimal(webhookData["final_amount"]);
                    else if (webhookData.ContainsKey("amount"))
                        monto = Convert.ToDecimal(webhookData["amount"]);

                    string externalTransactionId = webhookData.ContainsKey("external_transaction_id") ? 
                        webhookData["external_transaction_id"] : "";

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            IF EXISTS (SELECT 1 FROM Proveedor_Pagos WHERE Pago_ID = @PagoId)
                                UPDATE Proveedor_Pagos 
                                SET Estado = @Estado, 
                                    Monto = @Monto,
                                    Fecha_Actualizacion = @FechaActualizacion,
                                    Webhook_Data = @WebhookData
                                WHERE Pago_ID = @PagoId
                            ELSE
                                INSERT INTO Proveedor_Pagos (Pago_ID, External_Transaction_ID, Monto, Estado, Fecha_Creacion, Webhook_Data)
                                VALUES (@PagoId, @ExternalTransactionId, @Monto, @Estado, @FechaActualizacion, @WebhookData)";

                        command.Parameters.AddWithValue("@PagoId", pagoId);
                        command.Parameters.AddWithValue("@ExternalTransactionId", externalTransactionId);
                        command.Parameters.AddWithValue("@Monto", monto);
                        command.Parameters.AddWithValue("@Estado", status);
                        command.Parameters.AddWithValue("@FechaActualizacion", DateTime.Now);
                        command.Parameters.AddWithValue("@WebhookData", jsonSerializer.Serialize(webhookData));

                        int rowsAffected = command.ExecuteNonQuery();
                        LogDebug(String.Format("Filas afectadas en débito: {0}", rowsAffected));
                    }
                }

                LogDebug(String.Format("Débito procesado: {0}", pagoId));
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al procesar débito: {0}", ex.Message));
                throw;
            }
        }

        private void ProcesarSuscripcion(string suscripcionId, string status, dynamic webhookData)
        {
            try
            {
                LogDebug(String.Format("Procesando suscripción: {0}, Status: {1}", suscripcionId, status));

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            IF EXISTS (SELECT 1 FROM Proveedor WHERE Suscripcion_ID = @SuscripcionId)
                                UPDATE Proveedor
                                SET Estado = @Estado, 
                                    Fecha_Actualizacion = @FechaActualizacion,
                                    Webhook_Data = @WebhookData
                                WHERE Suscripcion_ID = @SuscripcionId
                            ELSE
                                INSERT INTO Proveedor (Suscripcion_ID, Estado, Fecha_Creacion, Webhook_Data)
                                VALUES (@SuscripcionId, @Estado, @FechaActualizacion, @WebhookData)";

                        command.Parameters.AddWithValue("@SuscripcionId", suscripcionId);
                        command.Parameters.AddWithValue("@Estado", status);
                        command.Parameters.AddWithValue("@FechaActualizacion", DateTime.Now);
                        command.Parameters.AddWithValue("@WebhookData", jsonSerializer.Serialize(webhookData));

                        int rowsAffected = command.ExecuteNonQuery();
                        LogDebug(String.Format("Filas afectadas en suscripción: {0}", rowsAffected));
                    }
                }

                LogDebug(String.Format("Suscripción procesada: {0}", suscripcionId));
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al procesar suscripción: {0}", ex.Message));
                throw;
            }
        }

        private void ProcesarNotificacionGenerica(string id, string tipo, string status, dynamic webhookData)
        {
            try
            {
                LogDebug(String.Format("Procesando notificación genérica: Tipo={0}, ID={1}, Status={2}", tipo, id, status));

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            INSERT INTO Proveedor_Notificaciones 
                            (ID, Tipo_Notificacion, Status, Fecha_Creacion, Webhook_Data)
                            VALUES (@Id, @Tipo, @Status, @FechaCreacion, @WebhookData)";

                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@Tipo", tipo);
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                        command.Parameters.AddWithValue("@WebhookData", jsonSerializer.Serialize(webhookData));

                        command.ExecuteNonQuery();
                    }
                }

                LogDebug(String.Format("Notificación genérica procesada: {0}", id));
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al procesar notificación genérica: {0}", ex.Message));
                throw;
            }
        }

        private static void LogDebug(string message)
        {
            try
            {
                string logEntry = String.Format("{0:yyyy-MM-dd HH:mm:ss} - DEBUG: {1}\n", DateTime.Now, message);
                File.AppendAllText(logPath, logEntry);
            }
            catch (Exception ex) 
            { 
                try
                {
                    string altLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "error.log");
                    File.AppendAllText(altLogPath, String.Format("{0:yyyy-MM-dd HH:mm:ss} - Error al escribir log: {1}\n", DateTime.Now, ex.Message));
                }
                catch { }
            }
        }

        private static void LogError(string message)
        {
            try
            {
                string logEntry = String.Format("{0:yyyy-MM-dd HH:mm:ss} - ERROR: {1}\n", DateTime.Now, message);
                File.AppendAllText(logPath, logEntry);
            }
            catch (Exception ex) 
            { 
                try
                {
                    string altLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "error.log");
                    File.AppendAllText(altLogPath, String.Format("{0:yyyy-MM-dd HH:mm:ss} - Error al escribir log: {1}\n", DateTime.Now, ex.Message));
                }
                catch { }
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
} 