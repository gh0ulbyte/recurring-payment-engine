using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace DebitosAutomaticos
{
    public partial class Resultado : System.Web.UI.Page
    {
    private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string resultado = Request.QueryString["resultado"];
            string debitoId = Request.QueryString["debitoId"];
            string tipoDebito = Request.QueryString["tipoDebito"];
            string transaccionId = Request.QueryString["transaccionId"];

            if (!string.IsNullOrEmpty(resultado))
            {
                MostrarResultado(resultado, debitoId, tipoDebito, transaccionId);
            }
            else
            {
                MostrarError("No se recibió información del resultado");
            }
        }
    }

    private void MostrarResultado(string resultado, string debitoId, string tipoDebito, string transaccionId)
    {
        try
        {
            
            if (resultado.StartsWith("{"))
            {
                var resultObj = _serializer.Deserialize<dynamic>(resultado);
                
                if (resultObj.ContainsKey("subscription_id") || resultObj.ContainsKey("adhesion_id") || resultObj.ContainsKey("transaction_id"))
                {
                   
                    pnlSuccess.Visible = true;
                    pnlDetails.Visible = true;
                    
                    string id = resultObj.ContainsKey("subscription_id") ? resultObj["subscription_id"] : 
                               resultObj.ContainsKey("adhesion_id") ? resultObj["adhesion_id"] : 
                               resultObj["transaction_id"];
                    
                    litDebitoId.Text = id;
                    litTipoDebito.Text = tipoDebito ?? "No especificado";
                    litTransaccionId.Text = transaccionId ?? "No especificado";
                    litEstado.Text = "ACTIVO";
                    litFechaCreacion.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    litProximoDebito.Text = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy");
                }
                else
                {
                   
                    pnlError.Visible = true;
                    litErrorMessage.Text = "Error en el formato de respuesta del servicio";
                }
            }
            else
            {
               
                pnlSuccess.Visible = true;
                pnlDetails.Visible = true;
                
                litDebitoId.Text = resultado;
                litTipoDebito.Text = tipoDebito ?? "No especificado";
                litTransaccionId.Text = transaccionId ?? "No especificado";
                litEstado.Text = "ACTIVO";
                litFechaCreacion.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                litProximoDebito.Text = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy");
            }
        }
        catch (Exception ex)
        {
            MostrarError("Error al procesar el resultado: " + ex.Message);
        }
    }

    private void MostrarError(string mensaje)
    {
        pnlError.Visible = true;
        litErrorMessage.Text = mensaje;
    }

    protected void btnConsultar_Click(object sender, EventArgs e)
    {
        try
        {
            string debitoId = litDebitoId.Text;
            if (string.IsNullOrEmpty(debitoId))
            {
                MostrarError("No hay ID de débito para consultar");
                return;
            }

            
            string resultado = DebitoAutomaticoService.ConsultarSuscripcion(debitoId);
            
            pnlInfo.Visible = true;
            litInfoMessage.Text = "Estado actualizado: " + resultado;
        }
        catch (Exception ex)
        {
            MostrarError("Error al consultar el estado: " + ex.Message);
        }
    }

    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        try
        {
            string debitoId = litDebitoId.Text;
            if (string.IsNullOrEmpty(debitoId))
            {
                MostrarError("No hay ID de débito para cancelar");
                return;
            }

            
            string resultado = DebitoAutomaticoService.CancelarSuscripcion(debitoId);
            
            
            ActualizarEstadoDebito(debitoId, "CANCELADO");
            
            pnlInfo.Visible = true;
            litInfoMessage.Text = "Débito automático cancelado exitosamente";
            litEstado.Text = "CANCELADO";
            
            
            btnConsultar.Visible = false;
            btnCancelar.Visible = false;
        }
        catch (Exception ex)
        {
            MostrarError("Error al cancelar el débito: " + ex.Message);
        }
    }

    private void ActualizarEstadoDebito(string debitoId, string estado)
    {
        try
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        UPDATE Proveedor
                        SET Estado = @Estado, Fecha_Modificacion = @FechaModificacion
                        WHERE Debito_ID = @DebitoId";

                    command.Parameters.AddWithValue("@Estado", estado);
                    command.Parameters.AddWithValue("@FechaModificacion", DateTime.Now);
                    command.Parameters.AddWithValue("@DebitoId", debitoId);

                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            
            System.Diagnostics.Debug.WriteLine("Error al actualizar estado: " + ex.Message);
        }
    }
    }
} 
