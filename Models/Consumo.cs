using System;

namespace MigradorCUAD.Models
{
    public class Consumo
    {
        public int NumeroConsumo { get; set; }
        public int NumeroSocio { get; set; }
        public DateTime Fecha { get; set; }
        public decimal ImporteTotal { get; set; }
        public string? EntidadCodigo { get; set; }
    }
}
