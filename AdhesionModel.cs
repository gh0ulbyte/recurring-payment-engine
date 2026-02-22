namespace DebitosAutomaticos.Models
{
    public class AdhesionModel
    {
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Documento { get; set; } = "";
        public decimal Monto { get; set; }
        public string Tasa { get; set; } = "1";
        public string Referencia { get; set; } = "";
    }
}
