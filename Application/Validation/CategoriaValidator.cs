using Implementador.Infrastructure;
using Implementador.Application.Validation.Common;
using Implementador.Application.Validation.Core;
using Implementador.Models;

namespace Implementador.Application.Validation;

public sealed class CategoriaValidator : RowValidatorBase
{
    public void Apply(
        ImplementationValidationResult result,
        IAppLogger log,
        ValidationReferenceData? snapshot = null)
    {
        if (result.DatosCategoriasValidadas.Count == 0)
        {
            return;
        }

        log.Separator();
        var categoriasPorEntidad = (snapshot ?? ValidationReferenceData.Empty).CategoriasPorEntidadRef;

        var categoriasFiltradas = FilterValidRows(
            ArchivoNombre.CategoriasSOCIOS,
            result.DatosCategoriasValidadas,
            log,
            (row, rowNumber) =>
            {
                var erroresFila = new List<string>();

                var entidad = RowValueReader.GetFirstValue(row, "Entidad");
                var codigoCategoria = RowValueReader.GetFirstValue(row, "Código Categoría", "Codigo Categoria");

                if (!string.IsNullOrWhiteSpace(entidad) && !string.IsNullOrWhiteSpace(codigoCategoria) && categoriasPorEntidad.Count > 0)
                {
                    if (!categoriasPorEntidad.TryGetValue(entidad.Trim(), out var categorias) ||
                        !categorias.Any(c => string.Equals(c.CodigoCategoria.Trim(), codigoCategoria.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        erroresFila.Add($"Código Categoría = \"{codigoCategoria}\" no existe en la base para la entidad \"{entidad.Trim()}\".");
                    }
                }

                return erroresFila;
            },
            out var rechazadas);

        if (rechazadas > 0)
            log.Info(ValidationLog.ReglaRechazadas(ArchivoNombre.CategoriasSOCIOS, rechazadas, rechazadas + categoriasFiltradas.Count));
        log.Info(ValidationLog.ListasParaImplementar(ArchivoNombre.CategoriasSOCIOS, categoriasFiltradas.Count));

        result.DatosCategoriasValidadas = categoriasFiltradas;
    }
}
