using ImplementadorCUAD.Models;
using ImplementadorCUAD.Infrastructure;
using System.Globalization;
using ImplementadorCUAD.Services.Common;
using ImplementadorCUAD.Services.Validation;

namespace ImplementadorCUAD.Services;

public sealed class CatalogoServiciosValidator(IAppDbContextFactory dbContextFactory) : RowValidatorBase
{
    private readonly IAppDbContextFactory _dbContextFactory = dbContextFactory;

    public void Apply(ImplementationValidationResult result, IAppLogger log)
    {
        if (result.DatosCatalogoServiciosValidados.Count == 0)
        {
            return;
        }

        List<CatalogoServicioCuadRef> catalogoCuad;
        try
        {
            using var db = _dbContextFactory.Create();
            catalogoCuad = db.GetCatalogoServiciosCuad();
        }
        catch (Exception ex)
        {
            log.Error($"Catalogo Servicios: no se pudo leer el catálogo de servicios de la base. {ex.Message}");
            result.DatosCatalogoServiciosValidados = [];
            return;
        }

        var catalogoPorEntidadServicio = catalogoCuad
            .ToDictionary(
                c => $"{c.Entidad.Trim()}|{c.Servicio.Trim()}",
                c => c,
                StringComparer.OrdinalIgnoreCase);

        var filtrado = FilterValidRows(
            "Catalogo Servicios",
            result.DatosCatalogoServiciosValidados,
            log,
            (row, rowNumber) =>
            {
                var erroresFila = new List<string>();
                var entidad = RowValueReader.GetFirstValue(row, "Entidad");
                var servicio = RowValueReader.GetFirstValue(row, "Servicio");
                var importeTexto = RowValueReader.GetFirstValue(row, "Importe");

                if (string.IsNullOrWhiteSpace(entidad) || string.IsNullOrWhiteSpace(servicio))
                {
                    erroresFila.Add("La entidad se encuentra vacia.");
                    return erroresFila;
                }

                var clave = $"{entidad.Trim()}|{servicio.Trim()}";
                if (!catalogoPorEntidadServicio.TryGetValue(clave, out var refCuad))
                {
                    erroresFila.Add($"servicio '{servicio}' no existe en la base para la entidad '{entidad}'.");
                    return erroresFila;
                }

                if (!ValueParsers.TryParseDecimalFlexible(importeTexto, out var importeArchivo))
                {
                    erroresFila.Add($"El importe '{importeTexto}' es invalido.");
                    return erroresFila;
                }

                var diferencia = Math.Abs(importeArchivo - refCuad.Importe);
                if (diferencia > 0.01m)
                {
                    erroresFila.Add($"El importe '{importeArchivo}' no coincide con la base ({refCuad.Importe}).");
                }

                return erroresFila;
            },
            out var rechazadas);

        if (rechazadas > 0)
        {
            log.Info($"Resumen validaciones Catalogo Servicios: aceptadas={filtrado.Count}, rechazadas={rechazadas}.");
        }

        result.DatosCatalogoServiciosValidados = filtrado;
    }

}

