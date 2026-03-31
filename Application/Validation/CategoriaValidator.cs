using Implementador.Infrastructure;
using Implementador.Application.Validation.Common;
using Implementador.Application.Validation.Core;

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
        var categoriasConCuotaSocial = (snapshot ?? ValidationReferenceData.Empty).CategoriasConCuotaSocial;

        var categoriasFiltradas = FilterValidRows(
            ArchivoNombre.CategoriasSOCIOS,
            result.DatosCategoriasValidadas,
            log,
            (row, rowNumber) =>
            {
                var erroresFila = new List<string>();

                var entidad = RowValueReader.GetFirstValue(row, "Entidad");
                var codigoCategoria = RowValueReader.GetFirstValue(row, "Mca_Nome");
                var conceptoDescuento = RowValueReader.GetFirstValue(row, "Mcc_COD_Entidad");

                if (string.IsNullOrWhiteSpace(conceptoDescuento))
                {
                    erroresFila.Add("El campo (Concepto Descuento) está vacío.");
                    return erroresFila;
                }

                if (!string.IsNullOrWhiteSpace(entidad) && !string.IsNullOrWhiteSpace(codigoCategoria) && categoriasConCuotaSocial.Count > 0)
                {
                    var key = $"{entidad.Trim()}|{codigoCategoria.Trim()}|{conceptoDescuento.Trim()}";
                    if (!categoriasConCuotaSocial.Contains(key))
                    {
                        erroresFila.Add($"El campo (Concepto Descuento) '{conceptoDescuento}' no es un código de cuota social vigente para la categoría '{codigoCategoria}' de la entidad '{entidad.Trim()}'.");
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
