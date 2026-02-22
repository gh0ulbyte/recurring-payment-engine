using System;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;

namespace DebitosAutomaticos
{
    public class TransaccionData
    {
        private static string logPath;

        static TransaccionData()
        {
            try
            {
                string appDataPath = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "App_Data");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                logPath = Path.Combine(appDataPath, "transaccion_debito.log");
                LogDebug("TransaccionData inicializado correctamente");
            }
            catch (Exception)
            {
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "transaccion_debito.log");
                try
                {
                    LogDebug("TransaccionData inicializado con ruta alternativa");
                }
                catch { }
            }
        }

        private static void LogDebug(string message)
        {
            try
            {
                string logEntry = String.Format("{0:yyyy-MM-dd HH:mm:ss} - {1}\n", DateTime.Now, message);
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

        public decimal Monto { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string NumeroDocumento { get; set; }
        public string ReferenciaExterna { get; set; }
        public string Token { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Referencia { get { return ReferenciaExterna; } }
        public string NombreContribuyente { get { return Nombre; } }
        public string DocumentoContribuyente { get { return NumeroDocumento; } }
        public string Tasa { get; set; }
        public string NumeroContribuyente { get; set; }
        public string Periodo { get; set; }

        public static TransaccionData GetTransaccionByToken(string token)
        {
            return ObtenerPorToken(token);
        }

        public static TransaccionData ObtenerPorToken(string token)
        {
            LogDebug(String.Format("Buscando transaccion con token: {0}", token));

            
            var datosPrueba = new[]
            {
                new { Token = "TOKEN_001", Nombre = "Juan Pérez", Email = "juan.perez@email.com", Monto = 1500.00m, NumeroDocumento = "12345678" },
                new { Token = "TOKEN_002", Nombre = "María García", Email = "maria.garcia@email.com", Monto = 2500.00m, NumeroDocumento = "87654321" },
                new { Token = "TOKEN_003", Nombre = "Carlos López", Email = "carlos.lopez@email.com", Monto = 3200.00m, NumeroDocumento = "11223344" }
            };

            
            foreach (var dato in datosPrueba)
            {
                if (dato.Token.Equals(token, StringComparison.OrdinalIgnoreCase))
                {
                    var transaccion = new TransaccionData
                    {
                        Tasa = "1",
                        Monto = dato.Monto,
                        Email = dato.Email,
                        Nombre = dato.Nombre,
                        NumeroDocumento = dato.NumeroDocumento,
                        ReferenciaExterna = String.Format("REF_{0}", token),
                        Token = token,
                        FechaTransaccion = DateTime.Now.AddDays(-1),
                        FechaVencimiento = DateTime.Now.AddDays(30),
                        NumeroContribuyente = dato.NumeroDocumento,
                        Periodo = DateTime.Now.ToString("yyyyMM")
                    };

                    LogDebug(String.Format("Transaccion de prueba encontrada: {0}, {1}, {2}", transaccion.Nombre, transaccion.Email, transaccion.Monto));
                    return transaccion;
                }
            }

            
            LogDebug(String.Format("Token {0} no encontrado, usando datos de prueba por defecto", token));
            return new TransaccionData
            {
                Tasa = "1",
                Monto = 1000.00m,
                Email = "test@email.com",
                Nombre = "Usuario de Prueba",
                NumeroDocumento = "99999999",
                ReferenciaExterna = String.Format("REF_{0}", DateTime.Now.ToString("yyyyMMddHHmmss")),
                Token = token,
                FechaTransaccion = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddDays(30),
                NumeroContribuyente = "99999999",
                Periodo = DateTime.Now.ToString("yyyyMM")
            };
        }

        public static TransaccionData ObtenerPorId(string transaccionId)
        {
            LogDebug(String.Format("Buscando transaccion con ID: {0}", transaccionId));

            
            var datosPrueba = new[]
            {
                new { Id = "1", Nombre = "Juan Pérez", Email = "juan.perez@email.com", Monto = 1500.00m, NumeroDocumento = "12345678" },
                new { Id = "2", Nombre = "María García", Email = "maria.garcia@email.com", Monto = 2500.00m, NumeroDocumento = "87654321" },
                new { Id = "3", Nombre = "Carlos López", Email = "carlos.lopez@email.com", Monto = 3200.00m, NumeroDocumento = "11223344" }
            };

            
            foreach (var dato in datosPrueba)
            {
                if (dato.Id.Equals(transaccionId, StringComparison.OrdinalIgnoreCase))
                {
                    var transaccion = new TransaccionData
                    {
                        Tasa = "1",
                        Monto = dato.Monto,
                        Email = dato.Email,
                        Nombre = dato.Nombre,
                        NumeroDocumento = dato.NumeroDocumento,
                        ReferenciaExterna = String.Format("REF_{0}", transaccionId),
                        Token = String.Format("TOKEN_{0}", transaccionId),
                        FechaTransaccion = DateTime.Now.AddDays(-1),
                        FechaVencimiento = DateTime.Now.AddDays(30),
                        NumeroContribuyente = dato.NumeroDocumento,
                        Periodo = DateTime.Now.ToString("yyyyMM")
                    };

                    LogDebug(String.Format("Transaccion de prueba encontrada por ID: {0}, {1}, {2}", transaccion.Nombre, transaccion.Email, transaccion.Monto));
                    return transaccion;
                }
            }

            
            LogDebug(String.Format("ID {0} no encontrado, usando datos de prueba por defecto", transaccionId));
            return new TransaccionData
            {
                Tasa = "1",
                Monto = 1500.00m,
                Email = "test@email.com",
                Nombre = "Usuario de Prueba",
                NumeroDocumento = "88888888",
                ReferenciaExterna = String.Format("REF_{0}", DateTime.Now.ToString("yyyyMMddHHmmss")),
                Token = String.Format("TOKEN_{0}", transaccionId),
                FechaTransaccion = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddDays(30),
                NumeroContribuyente = "88888888",
                Periodo = DateTime.Now.ToString("yyyyMM")
            };
        }

        public static void GuardarToken(string token)
        {
            LogDebug(String.Format("Guardando token (SIMULADO): {0}", token));
            
        }

        public static void Insertar(string debitoId, string tipoDebito)
        {
            LogDebug(String.Format("Insertando débito automático (SIMULADO): {0}, Tipo: {1}", debitoId, tipoDebito));
            
        }
    }
} 
