namespace ImplementadorCUAD.Models
{
    public class ImportarConsumoCab
    {
        public int Id { get; set; }
        public string? Entidad { get; set; }
        public int NroSocio { get; set; }
        public long? Cuit { get; set; }
        public int? NroPuesto { get; set; }
        public long CodigoConsumo { get; set; }
        public int CuotasPendientes { get; set; }
        public decimal MontoDeuda { get; set; }
        public int ConceptoDescuento { get; set; }
        public int? ConIdGenerado { get; set; }
        public bool Procesado { get; set; }
        public DateTime? FechaProceso { get; set; }
        public string? UsuarioProceso { get; set; }
        public string? Observacion { get; set; }
        public bool Ignorar { get; set; }
    }
}