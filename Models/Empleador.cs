namespace Implementador.Models
{
    public class Empleador
    {
        public int Id { get; set; }
        public int EmrId { get; set; }
        public string? Nombre { get; set; }
        /// <summary>
        /// Connection string de la base de destino para este empleador (cuando se carga desde config).
        /// </summary>
        public string? ConnectionString { get; set; }
        public override string ToString()
        {
            return Nombre ?? string.Empty;
        }
    }
}

