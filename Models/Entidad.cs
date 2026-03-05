namespace ImplementadorCUAD.Models
{
    public class Entidad
    {
        public int Id { get; set; }
        public int EntId { get; set; }
        public string? Nombre { get; set; }
        public override string ToString()
        {
            return Nombre ?? string.Empty;
        }
    }
}
