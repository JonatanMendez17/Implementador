using Implementador.Models;
using Implementador.Infrastructure;
using Implementador.Application.Validation.Common;
using Implementador.Application.Validation.Core;

namespace Implementador.Application.Validation;

public sealed class ConsumosValidator(IAppDbContextFactory dbContextFactory) : RowValidatorBase
{
    private readonly IAppDbContextFactory _dbContextFactory = dbContextFactory;

    public void Apply(ImplementationValidationResult result, IAppLogger log, ValidationReferenceData? snapshot = null, string? targetConnectionString = null)
    {
        if (result.DatosConsumosValidados.Count == 0)
        {
            return;
        }

        log.Separator();
        var safeSnapshot = snapshot ?? ValidationReferenceData.Empty;
        var entidadesRef = safeSnapshot.EntidadesRef;
        var conceptosDescuentoVigentes = safeSnapshot.ConceptosDescuentoVigentes;

        var padronRechazadosPorSocio = result.PadronSociosRechazados;
        var padronPorSocio = result.DatosPadronValidados
            .Where(f => RowValueReader.TryGetFirstValue(f, out var nro, "Nro Socio") && !string.IsNullOrWhiteSpace(nro))
            .GroupBy(f => RowValueReader.GetFirstValue(f, "Nro Socio").Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var padronDisponible = padronPorSocio.Count > 0;
        if (!padronDisponible && padronRechazadosPorSocio.Count == 0)
        {
            log.Warn("Consumos: No se cargó archivo de Padrón de Socios. No se puede verificar que el Nro Socio corresponda al padrón.");
        }

        var entidad = entidadesRef.FirstOrDefault() ?? string.Empty;
        var dbChecker = new DbDuplicateChecker.Builder(_dbContextFactory, targetConnectionString, log)
            .Add("Codigo Consumo", db => db.GetCodigosConsumoExistentes(entidad))
            .Build();

        var codigosConsumoVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var consumosRechazadosPorCodigo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var consumosFiltrados = FilterValidRows(
            ArchivoNombre.Consumos,
            result.DatosConsumosValidados,
            log,
            (row, rowNumber) =>
            {
            var erroresFila = new List<string>();

            var entidadFila = RowValueReader.GetFirstValue(row, "Entidad");
            var nroSocio = RowValueReader.GetFirstValue(row, "Nro Socio");
            var cuitConsumo = RowValueReader.GetFirstValue(row, "CUIT");
            var beneficioConsumo = RowValueReader.GetFirstValue(row, "Beneficio");
            var codigoConsumo = RowValueReader.GetFirstValue(row, "Codigo Consumo", "Código Consumo");
            var conceptoDescuentoText = RowValueReader.GetFirstValue(row, "Concepto Descuento");

            if (!entidadesRef.Contains(entidadFila!.Trim()))
            {
                erroresFila.Add($"Entidad = \"{entidadFila}\" no existe en la base.");
            }

            if (padronDisponible)
            {
                if (!padronPorSocio.TryGetValue(nroSocio!.Trim(), out var filaPadron))
                {
                    if (padronRechazadosPorSocio.TryGetValue(nroSocio.Trim(), out var motivoRechazo))
                    {
                        consumosRechazadosPorCodigo[codigoConsumo!.Trim()] = string.Empty;
                        return SilentReject;
                    }
                    else
                        erroresFila.Add($"Nro Socio = \"{nroSocio}\" no existe en el padron de socio.");
                }
                else
                {
                    var cuitPadron = RowValueReader.GetFirstValue(filaPadron, "CUIT");
                    var beneficioPadron = RowValueReader.GetFirstValue(filaPadron, "Beneficio");

                    if (!ValueParsers.EqualsDigitsOnly(cuitConsumo, cuitPadron))
                    {
                        erroresFila.Add($"CUIT = \"{cuitConsumo}\" no coincide con el valor del padron \"{cuitPadron}\" para socio \"{nroSocio}\".");
                    }

                    if (!ValueParsers.EqualsTrimmed(beneficioConsumo, beneficioPadron))
                    {
                        erroresFila.Add($"Beneficio = \"{beneficioConsumo}\" no coincide con el valor del padron \"{beneficioPadron}\" para socio \"{nroSocio}\".");
                    }
                }
            }
            else if (padronRechazadosPorSocio.ContainsKey(nroSocio!.Trim()))
            {
                consumosRechazadosPorCodigo[codigoConsumo!.Trim()] = string.Empty;
                return SilentReject;
            }

            if (!codigosConsumoVistos.Add(codigoConsumo!.Trim()))
            {
                erroresFila.Add($"Codigo Consumo = \"{codigoConsumo}\" se encuentra duplicado en el archivo.");
            }
            else if (long.TryParse(codigoConsumo.Trim(), out var codigoLong) && dbChecker.ExisteEnBase("Codigo Consumo", codigoLong))
            {
                erroresFila.Add($"Codigo Consumo = \"{codigoConsumo}\" ya existe en la base del empleador.");
            }

            if (!string.IsNullOrWhiteSpace(conceptoDescuentoText) &&
                conceptosDescuentoVigentes.Count > 0)
            {
                var keyConcepto = $"{entidadFila.Trim()}|{conceptoDescuentoText.Trim()}";
                if (!conceptosDescuentoVigentes.Contains(keyConcepto))
                {
                    erroresFila.Add($"Concepto Descuento = \"{conceptoDescuentoText}\" no existe como código de descuento vigente en la base para la entidad \"{entidadFila.Trim()}\".");
                }
            }

            if (erroresFila.Count > 0 && !string.IsNullOrWhiteSpace(codigoConsumo))
                consumosRechazadosPorCodigo[codigoConsumo.Trim()] = erroresFila[0];

            return erroresFila;
            },
            out var rechazadas);

        if (rechazadas > 0)
            log.Info(ValidationLog.ReglaRechazadas(ArchivoNombre.Consumos, rechazadas, rechazadas + consumosFiltrados.Count));
        log.Info(ValidationLog.ListasParaImplementar(ArchivoNombre.Consumos, consumosFiltrados.Count));

        result.DatosConsumosValidados = consumosFiltrados;
        result.ConsumosRechazados = consumosRechazadosPorCodigo;
    }

}
