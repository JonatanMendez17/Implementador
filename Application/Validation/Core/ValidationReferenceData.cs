using Implementador.Models;

namespace Implementador.Application.Validation.Core;

public sealed class ValidationReferenceData
{
    public static ValidationReferenceData Empty { get; } = new();

    public HashSet<string> EntidadesRef { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ConceptosDescuentoVigentes { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, List<CategoriaRef>> CategoriasPorEntidadRef { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> CategoriasConCuotaSocial { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, CatalogoServicioRef> CatalogoPorEntidadServicio { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}


