namespace Implementador.Models
{
    public class CategoriaRef
    {
        public int Id { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public string CodigoCategoria { get; set; } = string.Empty;
        public string? NombreCategoria { get; set; }
        public bool EsPredeterminada { get; set; }
        public bool Habilitada { get; set; }
    }
}


