using ImplementadorCUAD.Models;
using ImplementadorCUAD.Infrastructure;
using System.Globalization;
using ImplementadorCUAD.Services.Common;

namespace ImplementadorCUAD.Services;

public sealed class ServiciosValidator(IAppDbContextFactory dbContextFactory)
{
    private readonly IAppDbContextFactory _dbContextFactory = dbContextFactory;

    public void Apply(ImplementationValidationResult result, IAppLogger log)
    {
        if (result.DatosServiciosValidados.Count == 0)
        {
            return;
        }

        HashSet<string> entidadesCuad;
        try
        {
            using var db = _dbContextFactory.Create();
            entidadesCuad = db.GetEntidad()
                .SelectMany(e => new[]
                {
                    e.Nombre?.Trim(),
                    e.EntId.ToString(CultureInfo.InvariantCulture)
                })
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            log.Error($"Consumos Servicios: no se pudo validar entidades de la base. {ex.Message}");
            result.DatosServiciosValidados = new List<Dictionary<string, string>>();
            return;
        }

        var padronPorSocio = result.DatosPadronValidados
            .Where(f => RowValueReader.TryGetFirstValue(f, out var nro, "Nro Socio") && !string.IsNullOrWhiteSpace(nro))
            .GroupBy(f => RowValueReader.GetFirstValue(f, "Nro Socio").Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var codigosConsumos = result.DatosConsumosValidados
            .Select(f => RowValueReader.GetFirstValue(f, "Codigo Consumo", "Código Consumo").Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var serviciosFiltrados = new List<Dictionary<string, string>>();
        var codigosServiciosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rechazadas = 0;

        for (int i = 0; i < result.DatosServiciosValidados.Count; i++)
        {
            var row = result.DatosServiciosValidados[i];
            var rowNumber = i + 2;
            var erroresFila = new List<string>();

            var entidad = RowValueReader.GetFirstValue(row, "Entidad");
            var nroSocio = RowValueReader.GetFirstValue(row, "Nro de Socio", "Nro Socio");
            var cuitServicio = RowValueReader.GetFirstValue(row, "CUIT");
            var beneficioServicio = RowValueReader.GetFirstValue(row, "Nro Beneficio", "Beneficio");
            var codigoConsumo = RowValueReader.GetFirstValue(row, "Codigo Consumo", "Código Consumo");

            if (string.IsNullOrWhiteSpace(entidad) || !entidadesCuad.Contains(entidad.Trim()))
            {
                erroresFila.Add($"La entidad '{entidad}' no existe en la base.");
            }

            if (string.IsNullOrWhiteSpace(nroSocio) || !padronPorSocio.TryGetValue(nroSocio.Trim(), out var filaPadron))
            {
                erroresFila.Add($"El socio '{nroSocio}' no existe o no corresponde al padron socios.");
            }
            else
            {
                var cuitPadron = RowValueReader.GetFirstValue(filaPadron, "CUIT");
                var beneficioPadron = RowValueReader.GetFirstValue(filaPadron, "Beneficio");

                if (!ValueParsers.EqualsDigitsOnly(cuitServicio, cuitPadron))
                {
                    erroresFila.Add($"El CUIT no coincide con padron para socio '{nroSocio}'.");
                }

                if (!ValueParsers.EqualsTrimmed(beneficioServicio, beneficioPadron))
                {
                    erroresFila.Add($"El Beneficio no coincide con padron para socio '{nroSocio}'.");
                }
            }

            if (string.IsNullOrWhiteSpace(codigoConsumo))
            {
                erroresFila.Add("EL campo 'codigo consumo' se encuentra vacio.");
            }
            else
            {
                var codigoNormalizado = codigoConsumo.Trim();
                if (!codigosServiciosVistos.Add(codigoNormalizado))
                {
                    erroresFila.Add($"El codigo de consumo '{codigoConsumo}' se encuentra repetido en Consumos Servicios.");
                }

                if (codigosConsumos.Contains(codigoNormalizado))
                {
                    erroresFila.Add($"El codigo de consumo '{codigoConsumo}' ya existe en archivo Consumos.");
                }
            }

            if (erroresFila.Count == 0)
            {
                serviciosFiltrados.Add(row);
            }
            else
            {
                rechazadas++;
                log.Warn($"Consumos Servicios row {rowNumber}: {string.Join(" | ", erroresFila)}");
            }
        }

        if (rechazadas > 0)
        {
            log.Info($"Resumen validacion Consumos Servicios: aceptadas={serviciosFiltrados.Count}, rechazadas={rechazadas}.");
        }

        result.DatosServiciosValidados = serviciosFiltrados;
    }

}

