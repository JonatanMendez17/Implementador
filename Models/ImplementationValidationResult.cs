namespace Implementador.Models
{
    public class ImplementationValidationResult
    {
        public List<Dictionary<string, string>> DatosPadronValidados { get; set; } = new();
        public List<Dictionary<string, string>> DatosCategoriasValidadas { get; set; } = new();
        public List<Dictionary<string, string>> DatosConsumosValidados { get; set; } = new();
        public List<Dictionary<string, string>> DatosConsumosDetalleValidados { get; set; } = new();
        public List<Dictionary<string, string>> DatosCatalogoServiciosValidados { get; set; } = new();
        public List<Dictionary<string, string>> DatosServiciosValidados { get; set; } = new();
        public bool HasLoadedData { get; set; }
        public Dictionary<string, string> PadronSociosRechazados { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ConsumosRechazados { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}

