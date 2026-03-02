namespace ImplementadorCUAD.Models
{
    public class CatalogoServicioCuadRef
    {
        public int Id { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public decimal Importe { get; set; }
        public int? CodigoConceptoDescuento { get; set; }
        public bool Habilitado { get; set; }
    }
}

