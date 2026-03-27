using Implementador.Application.Validation;
using Implementador.Application.Validation.Core;
using Implementador.Models;
using Implementador.Tests.Helpers;
using Xunit;

namespace Implementador.Tests.Validators;

public class ConsumosValidatorTests
{
    private readonly ConsumosValidator _sut = new();
    private readonly FakeLogger _log = new();

    private static Dictionary<string, string> Fila(params (string key, string value)[] pares) =>
        pares.ToDictionary(p => p.key, p => p.value);

    private static ImplementationValidationResult ResultadoConPadron(
        List<Dictionary<string, string>> padron,
        List<Dictionary<string, string>> consumos)
    {
        return new ImplementationValidationResult
        {
            DatosPadronValidados = padron,
            DatosConsumosValidados = consumos,
            HasLoadedData = true
        };
    }

    private static ValidationReferenceData SnapshotConEntidad(string entidad) =>
        new()
        {
            EntidadesRef = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { entidad },
            ConceptosDescuentoVigentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };

    private static Dictionary<string, string> FilaPadron(string nroSocio, string cuit = "", string beneficio = "") =>
        Fila(
            ("Entidad", "BDI"),
            ("Nro Socio", nroSocio),
            ("CUIT", cuit),
            ("Beneficio", beneficio),
            ("Documento", "12345678"),
            ("Código Categoría", "A")
        );

    // ── Tests de filas válidas ─────────────────────────────────────────────────

    [Fact]
    public void Apply_FilaValida_SeAcepta()
    {
        var padron = new List<Dictionary<string, string>> { FilaPadron("10") };
        var consumos = new List<Dictionary<string, string>>
        {
            Fila(
                ("Entidad", "BDI"),
                ("Nro Socio", "10"),
                ("Código Consumo", "9001"),
                ("Cuotas Pendientes", "3"),
                ("Monto Deuda", "900"),
                ("Concepto Descuento", "")
            )
        };
        var result = ResultadoConPadron(padron, consumos);

        _sut.Apply(result, _log, SnapshotConEntidad("BDI"));

        Assert.Single(result.DatosConsumosValidados);
    }

    [Fact]
    public void Apply_CodigoConsumoDuplicado_SoloSeAceptaElPrimero()
    {
        var padron = new List<Dictionary<string, string>> { FilaPadron("10"), FilaPadron("11") };
        var consumos = new List<Dictionary<string, string>>
        {
            Fila(("Entidad", "BDI"), ("Nro Socio", "10"), ("Código Consumo", "9001"),
                 ("Cuotas Pendientes", "1"), ("Monto Deuda", "100"), ("Concepto Descuento", "")),
            Fila(("Entidad", "BDI"), ("Nro Socio", "11"), ("Código Consumo", "9001"),
                 ("Cuotas Pendientes", "1"), ("Monto Deuda", "100"), ("Concepto Descuento", ""))
        };
        var result = ResultadoConPadron(padron, consumos);

        _sut.Apply(result, _log, SnapshotConEntidad("BDI"));

        Assert.Single(result.DatosConsumosValidados);
        Assert.True(_log.HasWarnings);
    }

    [Fact]
    public void Apply_EntidadNoExisteEnReferencia_SeRechaza()
    {
        var padron = new List<Dictionary<string, string>> { FilaPadron("10") };
        var consumos = new List<Dictionary<string, string>>
        {
            Fila(("Entidad", "ENTIDAD_DESCONOCIDA"), ("Nro Socio", "10"),
                 ("Código Consumo", "9001"), ("Cuotas Pendientes", "1"),
                 ("Monto Deuda", "100"), ("Concepto Descuento", ""))
        };
        var result = ResultadoConPadron(padron, consumos);

        _sut.Apply(result, _log, SnapshotConEntidad("BDI"));

        Assert.Empty(result.DatosConsumosValidados);
    }

    [Fact]
    public void Apply_NroSocioNoExisteEnPadron_SeRechaza()
    {
        var padron = new List<Dictionary<string, string>> { FilaPadron("10") };
        var consumos = new List<Dictionary<string, string>>
        {
            Fila(("Entidad", "BDI"), ("Nro Socio", "99"),
                 ("Código Consumo", "9001"), ("Cuotas Pendientes", "1"),
                 ("Monto Deuda", "100"), ("Concepto Descuento", ""))
        };
        var result = ResultadoConPadron(padron, consumos);

        _sut.Apply(result, _log, SnapshotConEntidad("BDI"));

        Assert.Empty(result.DatosConsumosValidados);
    }

    [Fact]
    public void Apply_CUITNoCoincideConPadron_SeRechaza()
    {
        var padron = new List<Dictionary<string, string>>
        {
            Fila(("Entidad","BDI"), ("Nro Socio","10"), ("CUIT","20123456789"),
                 ("Beneficio",""), ("Documento","12345678"), ("Código Categoría","A"))
        };
        var consumos = new List<Dictionary<string, string>>
        {
            Fila(("Entidad", "BDI"), ("Nro Socio", "10"), ("CUIT", "27999999994"),
                 ("Código Consumo", "9001"), ("Cuotas Pendientes", "1"),
                 ("Monto Deuda", "100"), ("Concepto Descuento", ""))
        };
        var result = ResultadoConPadron(padron, consumos);

        _sut.Apply(result, _log, SnapshotConEntidad("BDI"));

        Assert.Empty(result.DatosConsumosValidados);
    }

    [Fact]
    public void Apply_SinConsumosEnResultado_NoHaceNada()
    {
        var result = new ImplementationValidationResult
        {
            DatosConsumosValidados = [],
            DatosPadronValidados = [],
            HasLoadedData = false
        };

        _sut.Apply(result, _log, SnapshotConEntidad("BDI"));

        Assert.Empty(result.DatosConsumosValidados);
        Assert.False(_log.HasErrors);
    }
}
