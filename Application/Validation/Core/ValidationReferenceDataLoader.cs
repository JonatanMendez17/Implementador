using System.Globalization;
using Implementador.Data;
using Implementador.Infrastructure;
using Implementador.Models;

namespace Implementador.Application.Validation.Core;

public sealed class ValidationReferenceDataLoader(IAppDbContextFactory dbContextFactory)
{
    private readonly IAppDbContextFactory _dbContextFactory = dbContextFactory;

    public ValidationReferenceData Load(bool includeCatalogoServicios = true)
    {
        using var db = _dbContextFactory.Create();

        var entidadesRef = db.GetEntidad()
            .SelectMany(e => new[]
            {
                e.Nombre?.Trim(),
                e.EntId.ToString(CultureInfo.InvariantCulture)
            })
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categoriasPorEntidadRef = db.GetCategoriasRef()
            .GroupBy(c => c.Entidad.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var catalogoPorEntidadServicio = includeCatalogoServicios
            ? db.GetCatalogoServiciosRef()
                .ToDictionary(
                    c => $"{c.Entidad.Trim()}|{c.Servicio.Trim()}",
                    c => c,
                    StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, CatalogoServicioRef>(StringComparer.OrdinalIgnoreCase);

        return new ValidationReferenceData
        {
            EntidadesRef = entidadesRef,
            ConceptosDescuentoVigentes = db.GetConceptosDescuentoVigentesParaConsumos(),
            CategoriasPorEntidadRef = categoriasPorEntidadRef,
            CategoriasConCuotaSocial = db.GetCategoriasConCuotaSocialVigente(),
            CatalogoPorEntidadServicio = catalogoPorEntidadServicio
        };
    }
}


