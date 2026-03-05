namespace ImplementadorCUAD.Models
{
    public class ImportarPadronSocio
    {
        public int Id { get; set; }
        public string Entidad { get; set; } = null!;
        public int NroSocio { get; set; }
        public int Documento { get; set; }
        public long? Cuit { get; set; }   
        public int? NroPuesto { get; set; } 
        public string CodigoCategoria { get; set; } = null!;
        public DateTime FechaAltaSocio { get; set; }

        // Campos de proceso
        public int? SocIdGenerado { get; set; }
        public bool Procesado { get; set; }
        public DateTime? FechaProceso { get; set; }
        public string? UsuarioProceso { get; set; }
        public string? Observacion { get; set; }
        public bool Ignorar { get; set; }
    }
}
