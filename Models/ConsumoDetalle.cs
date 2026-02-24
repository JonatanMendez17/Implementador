namespace MigradorCUAD.Models
{
    public class ConsumoDetalle
    {
        public int NumeroConsumo { get; set; }
        public int Periodo { get; set; }
        public decimal Importe { get; set; }
        public DateTime PrimerVencimiento { get; set; }
        public string? EntidadCodigo { get; set; }
    }
}
