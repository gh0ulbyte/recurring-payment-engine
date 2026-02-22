using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Web;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Globalization;

namespace DebitosAutomaticos
{
    
    public class SubscriptionRequest
    {
        public string collector_id { get; set; }
        public string currency_id { get; set; }
        public string external_reference { get; set; }
        public string due_date { get; set; }
        public List<TransactionDetail> details { get; set; }
        public Payer payer { get; set; }
        public string return_url { get; set; }
        public string notification_url { get; set; }
        public string frequency { get; set; }
        public string frequency_type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
    }

    
    public class AdhesionRequest
    {
        public string collector_id { get; set; }
        public string currency_id { get; set; }
        public string external_reference { get; set; }
        public string due_date { get; set; }
        public List<TransactionDetail> details { get; set; }
        public Payer payer { get; set; }
        public string return_url { get; set; }
        public string notification_url { get; set; }
        public string frequency { get; set; }
        public string frequency_type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
    }

    
    public class PaymentAndSubscriptionRequest
    {
        public string collector_id { get; set; }
        public string currency_id { get; set; }
        public string external_transaction_id { get; set; }
        public string due_date { get; set; }
        public List<TransactionDetail> details { get; set; }
        public Payer payer { get; set; }
        public string return_url { get; set; }
        public string notification_url { get; set; }
        public string frequency { get; set; }
        public string frequency_type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string subscription_type { get; set; } 
    }

    public class TransactionDetail
    {
        public string external_reference { get; set; }
        public string concept_id { get; set; }
        public string concept_description { get; set; }
        public decimal amount { get; set; }
    }

    public class Payer
    {
        public string name { get; set; }
        public string email { get; set; }
        public Identification identification { get; set; }
    }

    public class Identification
    {
        public string type { get; set; }
        public string number { get; set; }
        public string country { get; set; }
    }

    public class DebitoAutomaticoService
    {
        private static readonly JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        private static readonly string ProveedorBaseUrl;
        private static readonly string ProveedorAuthUrl;
        private static readonly string TokenEndpoint;
        private static readonly string SubscriptionUrl;
        private static readonly string AdhesionUrl;
        private static readonly string PaymentAndSubscriptionUrl;
        private static readonly string Username;
        private static readonly string Password;
        private static readonly string ClientId;
        private static readonly string ClientSecret;
        private static readonly string NotificationUrl;
        private static readonly string ReturnUrl;
        private static readonly string CollectorId;
        private static readonly string ConnectionString;
        private static readonly string ProveedorApiUrl;
        private static readonly string ProveedorApiKey;
        private static string logPath;

        static DebitoAutomaticoService()
        {
            try
            {
                
                ProveedorBaseUrl = GetConfigValue("Proveedor:BaseUrl");
                ProveedorAuthUrl = GetConfigValue("Proveedor:AuthUrl");
                TokenEndpoint = GetConfigValue("Proveedor:TokenEndpoint");
                SubscriptionUrl = GetConfigValue("Proveedor:SubscriptionUrl");
                AdhesionUrl = GetConfigValue("Proveedor:AdhesionUrl");
                PaymentAndSubscriptionUrl = GetConfigValue("Proveedor:PaymentAndSubscriptionUrl");
                Username = GetConfigValue("Proveedor:Username");
                Password = GetConfigValue("Proveedor:Password");
                ClientId = GetConfigValue("Proveedor:ClientId");
                ClientSecret = GetConfigValue("Proveedor:ClientSecret");
                NotificationUrl = GetConfigValue("Proveedor:NotificationUrl");
                ReturnUrl = GetConfigValue("Proveedor:ReturnUrl");
                CollectorId = GetConfigValue("Proveedor:CollectorId");
                
                var connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (connStr == null)
                {
                    throw new ConfigurationErrorsException("La cadena de conexión 'DefaultConnection' no está configurada");
                }
                ConnectionString = connStr.ConnectionString;
                
                ProveedorApiUrl = ConfigurationManager.AppSettings["ProveedorApiUrl"];
                ProveedorApiKey = ConfigurationManager.AppSettings["ProveedorApiKey"];

                if (string.IsNullOrEmpty(ConnectionString))
                {
                    throw new ConfigurationErrorsException("La cadena de conexión está vacía");
                }

               
                string appDataPath = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "App_Data");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                logPath = Path.Combine(appDataPath, "debito_automatico.log");

                LogDebug("DebitoAutomaticoService inicializado correctamente");
            }
            catch (Exception ex)
            {
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "debito_automatico.log");
                LogError("Error al inicializar DebitoAutomaticoService: " + ex.Message);
                throw;
            }
        }

        private static string GetConfigValue(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new ConfigurationErrorsException(String.Format("La configuración '{0}' no está definida", key));
            }
            return value;
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

        public static string ProcesarDebito(string token, string tipoDebito, string transaccionId)
        {
            try
            {
                LogDebug(String.Format("Iniciando procesamiento de débito automático - Token: {0}, Tipo: {1}, Transacción: {2}", token, tipoDebito, transaccionId));

                /
                var transaccionData = ObtenerTransaccionPorId(transaccionId);
                if (transaccionData == null)
                {
                    throw new Exception(String.Format("No se encontró la transacción con ID: {0}", transaccionId));
                }

                LogDebug(String.Format("Datos de transacción obtenidos: {0}, {1}, {2}", transaccionData.Nombre, transaccionData.Email, transaccionData.Monto));

                
                string accessToken = GetAccessToken();
                LogDebug("Token de acceso obtenido correctamente");

                
                string resultado;
                switch (tipoDebito.ToLower())
                {
                    case "suscripcion":
                        resultado = CrearSuscripcion(transaccionData, accessToken);
                        break;
                    case "adhesion":
                        resultado = CrearAdhesion(transaccionData, accessToken);
                        break;
                    case "pago_y_recurrencia":
                        resultado = CrearPagoYRecurrencia(transaccionData, accessToken);
                        break;
                    default:
                        throw new ArgumentException(String.Format("Tipo de débito no válido: {0}", tipoDebito));
                }

                
                GuardarResultadoDebito(transaccionId, tipoDebito, resultado, accessToken);

                LogDebug(String.Format("Débito automático procesado exitosamente: {0}", resultado));
                return resultado;
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error en ProcesarDebito: {0}", ex.Message));
                LogError(String.Format("StackTrace: {0}", ex.StackTrace));
                throw;
            }
        }

        private static TransaccionData ObtenerTransaccionPorId(string transaccionId)
        {
            LogDebug(String.Format("Buscando transacción con ID: {0}", transaccionId));

            using (var connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    LogDebug("Conexión a base de datos abierta");

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            SELECT TOP 1
                                Transaccion_Tasa as Tasa,
                                Transaccion_Valor as Monto,
                                Transaccion_Mail as Email,
                                Transaccion_Razon as Nombre,
                                Transaccion_Cuenta as NumeroDocumento,
                                Transaccion_Recibo as ReferenciaExterna,
                                Transaccion_Token as Token,
                                Transaccion_Fecha as FechaTransaccion,
                                DATEADD(day, 30, Transaccion_Fecha) as FechaVencimiento
                            FROM Proveedor
                            WHERE Transaccion_ID = @TransaccionId";

                        command.Parameters.AddWithValue("@TransaccionId", transaccionId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var transaccion = new TransaccionData
                                {
                                    Tasa = reader["Tasa"].ToString(),
                                    Monto = Convert.ToDecimal(reader["Monto"]),
                                    Email = reader["Email"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    NumeroDocumento = reader["NumeroDocumento"].ToString(),
                                    ReferenciaExterna = reader["ReferenciaExterna"].ToString(),
                                    Token = reader["Token"].ToString(),
                                    FechaTransaccion = Convert.ToDateTime(reader["FechaTransaccion"]),
                                    FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"])
                                };

                                LogDebug(String.Format("Transacción encontrada: {0}, {1}, {2}", transaccion.Nombre, transaccion.Email, transaccion.Monto));
                                return transaccion;
                            }
                        }
                    }
                }
                            catch (Exception ex)
            {
                LogError(String.Format("Error al obtener transacción: {0}", ex.Message));
                throw;
            }
            }

            LogError(String.Format("No se encontró la transacción con ID: {0}", transaccionId));
            return null;
        }

        private static string GetAccessToken()
        {
            try
            {
                LogDebug("Obteniendo token de acceso de Proveedor");

                var authUrl = ProveedorAuthUrl.TrimEnd('/') + TokenEndpoint;
                LogDebug(String.Format("URL de autenticación: {0}", authUrl));

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    
                    var formData = new Dictionary<string, string>
                    {
                        { "grant_type", "password" },
                        { "username", Username },
                        { "password", Password },
                        { "client_id", ClientId },
                        { "client_secret", ClientSecret }
                    };

                    var formDataString = String.Join("&", formData.Select(kvp => 
                        String.Format("{0}={1}", 
                            HttpUtility.UrlEncode(kvp.Key), 
                            HttpUtility.UrlEncode(kvp.Value)
                        )
                    ).ToArray());

                    LogDebug(String.Format("Request de token: {0}", formDataString));
                    var response = client.UploadString(authUrl, formDataString);
                    LogDebug(String.Format("Respuesta de token: {0}", response));

                    var tokenResponse = jsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    return tokenResponse["access_token"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error en GetAccessToken: {0}", ex.ToString()));
                throw;
            }
        }

        private static string CrearSuscripcion(TransaccionData transaccion, string accessToken)
        {
            try
            {
                LogDebug("Creando suscripción en Proveedor");

                var subscriptionRequest = new
                {
                    type = "subscription",
                    collector_id = CollectorId,
                    currency_id = "ARS",
                    recurrence = "monthly",
                    detail = new
                    {
                        external_reference = transaccion.ReferenciaExterna,
                        concept_id = transaccion.Tasa,
                        concept_description = ObtenerNombreTributoPorId(transaccion.Tasa),
                        amount = transaccion.Monto
                    },
                    payer = new
                    {
                        name = transaccion.Nombre,
                        email = transaccion.Email,
                        identification = new
                        {
                            type = MapearTipoDocumento("DNI"),
                            number = transaccion.NumeroDocumento,
                            country = "ARG"
                        }
                    },
                    return_url = ReturnUrl,
                    notification_url = NotificationUrl,
                    due_day = 20,
                    periods = 0
                };

                string jsonData = jsonSerializer.Serialize(subscriptionRequest);
                LogDebug(String.Format("Datos de suscripción: {0}", jsonData));

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var subscriptionUrl = ProveedorBaseUrl.TrimEnd('/') + SubscriptionUrl;
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;

                    string response = client.UploadString(subscriptionUrl, jsonData);
                    LogDebug(String.Format("Respuesta de suscripción: {0}", response));

                    var responseDict = jsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    return responseDict["id"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error al crear suscripción: {0}", ex.ToString()));
                throw;
            }
        }

        private static string CrearAdhesion(TransaccionData transaccion, string accessToken)
        {
            try
            {
                LogDebug("Creando adhesión en Proveedor");

                var adhesionRequest = new
                {
                    type = "adhesion",
                    notification_url = NotificationUrl,
                    currency_id = "ARS",
                    collector_id = CollectorId,
                    detail = new
                    {
                        external_reference = transaccion.ReferenciaExterna,
                        concept_id = transaccion.Tasa,
                        concept_description = ObtenerNombreTributoPorId(transaccion.Tasa)
                    },
                    payer = new
                    {
                        name = transaccion.Nombre,
                        email = transaccion.Email,
                        identification = new
                        {
                            type = MapearTipoDocumento("DNI"),
                            number = transaccion.NumeroDocumento,
                            country = "ARG"
                        }
                    }
                };

                string jsonData = jsonSerializer.Serialize(adhesionRequest);
                LogDebug(String.Format("Datos de adhesión: {0}", jsonData));

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var adhesionUrl = ProveedorBaseUrl.TrimEnd('/') + AdhesionUrl;
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;

                    string response = client.UploadString(adhesionUrl, jsonData);
                    LogDebug(String.Format("Respuesta de adhesión: {0}", response));

                    var responseDict = jsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    return responseDict["id"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error al crear adhesión: {0}", ex.ToString()));
                throw;
            }
        }

        private static string CrearPagoYRecurrencia(TransaccionData transaccion, string accessToken)
        {
            try
            {
                LogDebug("Creando pago online + recurrencia en Proveedor");

                var paymentRequest = new
                {
                    collector_id = CollectorId,
                    currency_id = "ARS",
                    external_reference = transaccion.ReferenciaExterna,
                    due_date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                    detail = new
                    {
                        external_reference = transaccion.ReferenciaExterna,
                        concept_id = transaccion.Tasa,
                        concept_description = ObtenerNombreTributoPorId(transaccion.Tasa),
                        amount = transaccion.Monto
                    },
                    payer = new
                    {
                        name = transaccion.Nombre,
                        email = transaccion.Email,
                        identification = new
                        {
                            type = MapearTipoDocumento("DNI"),
                            number = transaccion.NumeroDocumento,
                            country = "ARG"
                        }
                    },
                    return_url = ReturnUrl,
                    notification_url = NotificationUrl,
                    subscription = new
                    {
                        type = "subscription",
                        recurrence = "monthly",
                        due_day = 20,
                        periods = 0
                    }
                };

                string jsonData = jsonSerializer.Serialize(paymentRequest);
                LogDebug(String.Format("Datos de pago + recurrencia: {0}", jsonData));

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var paymentUrl = ProveedorBaseUrl.TrimEnd('/') + PaymentAndSubscriptionUrl;
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;

                    string response = client.UploadString(paymentUrl, jsonData);
                    LogDebug(String.Format("Respuesta de pago + recurrencia: {0}", response));

                    var responseDict = jsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    return responseDict["id"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error al crear pago + recurrencia: {0}", ex.ToString()));
                throw;
            }
        }

        private static void GuardarResultadoDebito(string transaccionId, string tipoDebito, string resultado, string accessToken)
        {
            try
            {
                LogDebug(String.Format("Guardando resultado de débito automático: {0}", resultado));

                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            INSERT INTO Proveedor 
                            (Transaccion_ID, Tipo_Debito, Resultado, Token_Acceso, Fecha_Creacion, Estado)
                            VALUES (@TransaccionId, @TipoDebito, @Resultado, @TokenAcceso, @FechaCreacion, @Estado)";

                        command.Parameters.AddWithValue("@TransaccionId", transaccionId);
                        command.Parameters.AddWithValue("@TipoDebito", tipoDebito);
                        command.Parameters.AddWithValue("@Resultado", resultado);
                        command.Parameters.AddWithValue("@TokenAcceso", accessToken);
                        command.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                        command.Parameters.AddWithValue("@Estado", "ACTIVO");

                        command.ExecuteNonQuery();
                    }
                }

                LogDebug("Resultado de débito automático guardado correctamente");
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al guardar resultado de débito automático: {0}", ex.Message));
                
            }
        }

        private static string MapearTipoDocumento(string tipoDocumento)
        {
            if (string.IsNullOrEmpty(tipoDocumento))
            {
                return "DNI_ARG";
            }
            
            switch (tipoDocumento.ToUpper())
            {
                case "DNI": 
                    return "DNI_ARG";
                case "CUIL": 
                    return "CUIL_ARG";
                case "CUIT": 
                    return "CUIT_ARG";
                default: 
                    return "DNI_ARG";
            }
        }

      

        
        public static string CrearCobroAdhesion(string adhesionId, TransaccionData transaccion, string accessToken)
        {
            try
            {
                LogDebug(String.Format("Creando cobro para adhesión: {0}", adhesionId));

                var paymentRequest = new
                {
                    currency_id = "ARS",
                    collector_id = CollectorId,
                    external_transaction_id = transaccion.ReferenciaExterna + "_" + DateTime.Now.Ticks,
                    due_date = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss-0300"),
                    notification_url = NotificationUrl,
                    details = new[]
                    {
                        new
                        {
                            external_reference = ,
                            concept_id = transaccion.Tasa,
                            concept_description = ObtenerNombreTributoPorId(transaccion.Tasa),
                            amount = transaccion.Monto
                        }
                    }
                };

                string jsonData = jsonSerializer.Serialize(paymentRequest);
                LogDebug(String.Format("Datos de cobro: {0}", jsonData));

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var paymentUrl = ProveedorBaseUrl.TrimEnd('/') + "/suscripciones/adhesion/" + adhesionId + "/pago";
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                    client.Headers[HttpRequestHeader.CacheControl] = "no-cache";

                    string response = client.UploadString(paymentUrl, jsonData);
                    LogDebug(String.Format("Respuesta de cobro: {0}", response));

                    var responseDict = jsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    return responseDict["id"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al crear cobro de adhesión: {0}", ex.Message));
                throw;
            }
        }

        
        public static string ConsultarSuscripcion(string subscriptionId)
        {
            try
            {
                LogDebug(String.Format("Consultando suscripción: {0}", subscriptionId));

                string accessToken = GetAccessToken();

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                    
                    string response = client.DownloadString(ProveedorBaseUrl + SubscriptionUrl + "/" + subscriptionId);
                    LogDebug(String.Format("Respuesta de consulta: {0}", response));

                    return response;
                }
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al consultar suscripción: {0}", ex.Message));
                throw;
            }
        }

        public static string CancelarSuscripcion(string subscriptionId)
        {
            try
            {
                LogDebug(String.Format("Cancelando suscripción: {0}", subscriptionId));

                string accessToken = GetAccessToken();

                var cancelRequest = new
                {
                    status_detail = "Cancelación solicitada por el usuario"
                };

                string jsonData = jsonSerializer.Serialize(cancelRequest);

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                    
                    string response = client.UploadString(ProveedorBaseUrl + "/suscripciones/cancelar/" + subscriptionId, jsonData);
                    LogDebug(String.Format("Respuesta de cancelación: {0}", response));

                    return response;
                }
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al cancelar suscripción: {0}", ex.Message));
                throw;
            }
        }

        
        public static string CancelarAdhesion(string adhesionId)
        {
            try
            {
                LogDebug(String.Format("Cancelando adhesión: {0}", adhesionId));

                string accessToken = GetAccessToken();

                var cancelRequest = new
                {
                    status_detail = "Cancelación de adhesión solicitada por el usuario"
                };

                string jsonData = jsonSerializer.Serialize(cancelRequest);

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                    
                    string response = client.UploadString(ProveedorBaseUrl + "/suscripciones/cancelar/" + adhesionId, jsonData);
                    LogDebug(String.Format("Respuesta de cancelación de adhesión: {0}", response));

                    return response;
                }
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al cancelar adhesión: {0}", ex.Message));
                throw;
            }
        }

        
        public static string CancelarPago(string paymentId)
        {
            try
            {
                LogDebug(String.Format("Cancelando pago: {0}", paymentId));

                string accessToken = GetAccessToken();

                var cancelRequest = new
                {
                    status_detail = "Cancelación de pago solicitada por el usuario"
                };

                string jsonData = jsonSerializer.Serialize(cancelRequest);

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                    client.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                    
                    string response = client.UploadString(ProveedorBaseUrl + "/pagos/cancelar/" + paymentId, jsonData);
                    LogDebug(String.Format("Respuesta de cancelación de pago: {0}", response));

                    return response;
                }
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error al cancelar pago: {0}", ex.Message));
                throw;
            }
        }
    }

    
    public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class SubscriptionResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public string form_url { get; set; }
        public string type { get; set; }
        public string collector_id { get; set; }
        public string currency_id { get; set; }
        public string recurrence { get; set; }
        public int periods { get; set; }
        public int due_day { get; set; }
        public string start_date { get; set; }
        public string request_date { get; set; }
        public string last_update_date { get; set; }
    }

    public class AdhesionResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public string form_url { get; set; }
        public string type { get; set; }
        public string collector_id { get; set; }
        public string currency_id { get; set; }
        public string recurrence { get; set; }
        public int periods { get; set; }
        public int due_day { get; set; }
        public string start_date { get; set; }
        public string request_date { get; set; }
        public string last_update_date { get; set; }
    }

    public class PaymentAndSubscriptionResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public string form_url { get; set; }
        public string external_reference { get; set; }
        public string collector_id { get; set; }
        public string currency_id { get; set; }
        public decimal amount { get; set; }
        public string due_date { get; set; }
        public string request_date { get; set; }
        public string last_update_date { get; set; }
        public SubscriptionResponse subscription { get; set; }
    }
} 
