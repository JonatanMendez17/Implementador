namespace MigradorCUAD.Models
{
    public class ColumnaConfiguracion
    {
        public string Nombre { get; set; } = string.Empty;
        public string TipoDato { get; set; } = string.Empty; // int, decimal, fecha, texto
        public int LargoMaximo { get; set; }
    }
}

