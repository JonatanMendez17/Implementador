using ImplementadorCUAD.Models;
using ImplementadorCUAD.Infrastructure;
using System.Globalization;
using ImplementadorCUAD.Services.Common;

namespace ImplementadorCUAD.Services;

public sealed class ConsumosValidator
{
    private readonly IAppDbContextFactory _dbContextFactory;

    public ConsumosValidator(IAppDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public void Apply(ImplementationValidationResult result, IAppLogger log)
    {
        if (result.DatosConsumosValidados.Count == 0)
        {
            return;
        }

        HashSet<string> entidadesCuad;
        HashSet<string> conceptosDescuentoVigentes;
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

            conceptosDescuentoVigentes = db.GetConceptosDescuentoVigentesParaConsumos();
        }
        catch (Exception ex)
        {
            log.Error($"Consumos: no se pudo validar entidades de la base. {ex.Message}");
            result.DatosConsumosValidados = new List<Dictionary<string, string>>();
            conceptosDescuentoVigentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        var padronPorSocio = result.DatosPadronValidados
            .Where(f => RowValueReader.TryGetFirstValue(f, out var nro, "Nro Socio") && !string.IsNullOrWhiteSpace(nro))
            .GroupBy(f => RowValueReader.GetFirstValue(f, "Nro Socio").Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var consumosFiltrados = new List<Dictionary<string, string>>();
        var codigosConsumoVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rechazadas = 0;

        for (int i = 0; i < result.DatosConsumosValidados.Count; i++)
        {
            var row = result.DatosConsumosValidados[i];
            var rowNumber = i + 2;
            var erroresFila = new List<string>();

            var entidad = RowValueReader.GetFirstValue(row, "Entidad");
            var nroSocio = RowValueReader.GetFirstValue(row, "Nro Socio");
            var cuitConsumo = RowValueReader.GetFirstValue(row, "CUIT");
            var beneficioConsumo = RowValueReader.GetFirstValue(row, "Beneficio");
            var codigoConsumo = RowValueReader.GetFirstValue(row, "Codigo Consumo", "Código Consumo");
            var conceptoDescuentoText = RowValueReader.GetFirstValue(row, "Concepto Descuento");

            if (string.IsNullOrWhiteSpace(entidad) || !entidadesCuad.Contains(entidad.Trim()))
            {
                erroresFila.Add($"La entidad '{entidad}' no existe en la base.");
            }

            if (string.IsNullOrWhiteSpace(nroSocio) || !padronPorSocio.TryGetValue(nroSocio.Trim(), out var filaPadron))
            {
                erroresFila.Add($"El nro socio '{nroSocio}' no existe o no corresponde al padron.");
            }
            else
            {
                var cuitPadron = RowValueReader.GetFirstValue(filaPadron, "CUIT");
                var beneficioPadron = RowValueReader.GetFirstValue(filaPadron, "Beneficio");

                if (!ValueParsers.EqualsDigitsOnly(cuitConsumo, cuitPadron))
                {
                    erroresFila.Add($"El CUIT no coincide con padron para socio '{nroSocio}'.");
                }

                if (!ValueParsers.EqualsTrimmed(beneficioConsumo, beneficioPadron))
                {
                    erroresFila.Add($"El Beneficio no coincide con padron para socio '{nroSocio}'.");
                }
            }

            if (string.IsNullOrWhiteSpace(codigoConsumo))
            {
                erroresFila.Add("El campo 'codigo consumo' se encuentra vacio.");
            }
            else if (!codigosConsumoVistos.Add(codigoConsumo.Trim()))
            {
                erroresFila.Add($"El codigo de consumo '{codigoConsumo}' se encuentra repetido.");
            }

            if (!string.IsNullOrWhiteSpace(entidad) && !string.IsNullOrWhiteSpace(conceptoDescuentoText) &&
                conceptosDescuentoVigentes.Count > 0)
            {
                var keyConcepto = $"{entidad.Trim()}|{conceptoDescuentoText.Trim()}";
                if (!conceptosDescuentoVigentes.Contains(keyConcepto))
                {
                    erroresFila.Add($"El concepto de descuento '{conceptoDescuentoText}' no existe como código de descuento vigente en la base para la entidad '{entidad?.Trim()}'.");
                }
            }

            if (erroresFila.Count == 0)
            {
                consumosFiltrados.Add(row);
            }
            else
            {
                rechazadas++;
                log.Warn($"Consumos row {rowNumber}: {string.Join(" | ", erroresFila)}");
            }
        }

        if (rechazadas > 0)
        {
            log.Info($"Resumen validacion Consumos: aceptadas={consumosFiltrados.Count}, rechazadas={rechazadas}.");
        }

        result.DatosConsumosValidados = consumosFiltrados;
    }

}

