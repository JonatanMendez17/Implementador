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
        string entidad, string codigoCategoria) =>
        new()
        {
            ["Entidad"]           = entidad,
            ["Código Categoría"]  = codigoCategoria,
        };

    private static ValidationReferenceData SnapshotConCategorias(string entidad, params string[] codigos) =>
        new()
        {
            CategoriasPorEntidadRef = new Dictionary<string, List<CategoriaRef>>(StringComparer.OrdinalIgnoreCase)
            {
                [entidad] = codigos.Select(c => new CategoriaRef { Entidad = entidad, CodigoCategoria = c, Habilitada = true }).ToList()
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
    public void Apply_CodigoExisteEnBase_FilaAceptada()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [FilaCategoria("BDI", "ADH")],
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCategorias("BDI", "ADH"));

        Assert.Single(result.DatosCategoriasValidadas);
        Assert.False(_log.HasWarnings);
    }

    [Fact]
    public void Apply_CodigoNoExisteEnBase_FilaSeRechaza()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [FilaCategoria("BDI", "INVALIDA")],
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCategorias("BDI", "ADH"));

        Assert.Empty(result.DatosCategoriasValidadas);
        Assert.True(_log.HasWarnings);
    }

    [Fact]
    public void Apply_EntidadNoExisteEnBase_FilaSeRechaza()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [FilaCategoria("OTRA", "ADH")],
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCategorias("BDI", "ADH"));

        Assert.Empty(result.DatosCategoriasValidadas);
        Assert.True(_log.HasWarnings);
    }

    [Fact]
    public void Apply_SinSnapshot_NoValidaCategoria()
    {
        var result = new ImplementationValidationResult
        {
            DatosCategoriasValidadas = [FilaCategoria("BDI", "INVALIDA")],
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log);

        Assert.Single(result.DatosCategoriasValidadas);
    }
}
