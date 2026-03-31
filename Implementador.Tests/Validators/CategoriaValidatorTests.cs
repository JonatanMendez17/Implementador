using Implementador.Application.Validation;
using Implementador.Application.Validation.Core;
using Implementador.Infrastructure;
using Implementador.Models;
using Implementador.Tests.Helpers;
using Xunit;

namespace Implementador.Tests.Validators;

public class CategoriaValidatorTests
{
    private readonly FakeLogger _log = new();

    private static CategoriaValidator CrearSut() => new();

    private static Dictionary<string, string> FilaCategoria(
        string entidad, string mcaNome, string mccCodEntidad) =>
        new()
        {
            ["Entidad"]         = entidad,
            ["Mca_Nome"]        = mcaNome,
            ["Mcc_COD_Entidad"] = mccCodEntidad
        };

    private static ValidationReferenceData SnapshotConCuota(string entidad, string categoria, string codEntidad) =>
        new()
        {
            CategoriasConCuotaSocial = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                $"{entidad}|{categoria}|{codEntidad}"
            }
        };

    [Fact]
    public void Apply_SinCategorias_NoHaceNada()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [],
            HasLoadedData = false
        };

        CrearSut().Apply(result, _log);

        Assert.Empty(result.DatosCategoriasValidadas);
        Assert.False(_log.HasErrors);
    }

    [Fact]
    public void Apply_ConceptoDescuentoVigente_FilaAceptada()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [FilaCategoria("BDI", "ADH", "6619")],
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCuota("BDI", "ADH", "6619"));

        Assert.Single(result.DatosCategoriasValidadas);
        Assert.False(_log.HasWarnings);
    }

    [Fact]
    public void Apply_ConceptoDescuentoNoVigente_FilaSeRechaza()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [FilaCategoria("BDI", "ADH", "9999")],
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCuota("BDI", "ADH", "6619"));

        Assert.Empty(result.DatosCategoriasValidadas);
        Assert.True(_log.HasWarnings);
    }

    [Fact]
    public void Apply_ConceptoDescuentoVacio_FilaSeRechaza()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [FilaCategoria("BDI", "ADH", "")],
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCuota("BDI", "ADH", "6619"));

        Assert.Empty(result.DatosCategoriasValidadas);
        Assert.True(_log.HasWarnings);
    }

    [Fact]
    public void Apply_SinSnapshot_NoValidaCuota()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [FilaCategoria("BDI", "ADH", "9999")],
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log);

        Assert.Single(result.DatosCategoriasValidadas);
    }
}
