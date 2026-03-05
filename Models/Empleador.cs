namespace ImplementadorCUAD.Models
{
    public class Empleador
    {
        public int Id { get; set; }
        public int EmrId { get; set; }
        public string? Nombre { get; set; }
        public override string ToString()
        {
            return Nombre ?? string.Empty;
        }
    }
}
