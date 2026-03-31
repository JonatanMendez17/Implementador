using Implementador.Application.Validation;
using Implementador.Application.Validation.Core;
using Implementador.Data;
using Implementador.Infrastructure;
using Implementador.Models;
using Implementador.Tests.Helpers;
using Moq;
using Xunit;

namespace Implementador.Tests.Validators;

public class PadronValidatorTests
{
    private readonly Mock<IAppDbContextFactory> _factoryMock = new();
    private readonly Mock<IAppDbContext> _dbMock = new();
    private readonly FakeLogger _log = new();

    public PadronValidatorTests()
    {
        _factoryMock.Setup(f => f.Create()).Returns(_dbMock.Object);
        _factoryMock.Setup(f => f.Create(It.IsAny<string>())).Returns(_dbMock.Object);

        // Por defecto: todos los documentos existen en DB
        _dbMock
            .Setup(d => d.GetDocumentosExistentesEnEmpleadoBatch(It.IsAny<IEnumerable<long>>()))
            .Returns((IEnumerable<long> docs) => new HashSet<long>(docs));
    }

    private PadronValidator CrearSut() => new(_factoryMock.Object);

    private static Dictionary<string, string> Fila(params (string key, string value)[] pares) =>
        pares.ToDictionary(p => p.key, p => p.value);

    private static Dictionary<string, string> FilaPadron(
        string nroSocio, string documento, string categoria = "A", string entidad = "BDI") =>
        Fila(
            ("Entidad", entidad),
            ("Nro Socio", nroSocio),
            ("Fecha Alta Socio", "01/01/2020"),
            ("Documento", documento),
            ("Código Categoría", categoria)
        );

    private static Dictionary<string, string> FilaCategoria(string codigo, string entidad = "BDI") =>
        Fila(
            ("Entidad", entidad),
            ("Código Categoría", codigo),
            ("Categoría", $"Categoria {codigo}")
        );

    private static ValidationReferenceData SnapshotConCategoria(string entidad, string codigoCategoria) =>
        new()
        {
            EntidadesRef = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { entidad },
            CategoriasPorEntidadRef = new Dictionary<string, List<CategoriaRef>>(StringComparer.OrdinalIgnoreCase)
            {
                [entidad] = [new CategoriaRef { Entidad = entidad, CodigoCategoria = codigoCategoria, Habilitada = true }]
            }
        };

    // ── Tests válidos ─────────────────────────────────────────────────────────

    [Fact]
    public void Apply_SocioDuplicadoEnArchivo_SoloSeAceptaElPrimero()
    {
        var padron = new List<Dictionary<string, string>>
        {
            FilaPadron("10", "12345678"),
            FilaPadron("10", "87654321")   // mismo Nro Socio
        };
        var categorias = new List<Dictionary<string, string>> { FilaCategoria("A") };
        var result = new ImplementationValidationResult
        {
            DatosPadronValidados = padron,
            DatosCategoriasValidadas = categorias,
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCategoria("BDI", "A"));

        Assert.Single(result.DatosPadronValidados);
        Assert.True(_log.HasWarnings);
    }

    [Fact]
    public void Apply_DocumentoDuplicado_SegundoSocioSeRechaza()
    {
        var padron = new List<Dictionary<string, string>>
        {
            FilaPadron("10", "12345678"),
            FilaPadron("11", "12345678")   // mismo documento
        };
        var categorias = new List<Dictionary<string, string>> { FilaCategoria("A") };
        var result = new ImplementationValidationResult
        {
            DatosPadronValidados = padron,
            DatosCategoriasValidadas = categorias,
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCategoria("BDI", "A"));

        Assert.Single(result.DatosPadronValidados);
    }

    [Fact]
    public void Apply_CategoriaNoExisteEnArchivoCategorias_SeRechaza()
    {
        var padron = new List<Dictionary<string, string>>
        {
            FilaPadron("10", "12345678", "CATEGORIA_INVALIDA")
        };
        var categorias = new List<Dictionary<string, string>> { FilaCategoria("A") };
        var result = new ImplementationValidationResult
        {
            DatosPadronValidados = padron,
            DatosCategoriasValidadas = categorias,
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCategoria("BDI", "A"));

        Assert.Empty(result.DatosPadronValidados);
    }

    [Fact]
    public void Apply_SinPadron_NoHaceNada()
    {
        var result = new ImplementationValidationResult
        {
            DatosPadronValidados = [],
            DatosCategoriasValidadas = [],
            HasLoadedData = false
        };

        CrearSut().Apply(result, _log);

        Assert.Empty(result.DatosPadronValidados);
        Assert.False(_log.HasErrors);
    }

    [Fact]
    public void Apply_DocumentoNoExisteEnBase_FilaSeRechaza()
    {
        _dbMock
            .Setup(d => d.GetDocumentosExistentesEnEmpleadoBatch(It.IsAny<IEnumerable<long>>()))
            .Returns(new HashSet<long>()); // ningún documento existe

        var padron = new List<Dictionary<string, string>>
        {
            FilaPadron("10", "12345678")
        };
        var categorias = new List<Dictionary<string, string>> { FilaCategoria("A") };
        var result = new ImplementationValidationResult
        {
            DatosPadronValidados = padron,
            DatosCategoriasValidadas = categorias,
            HasLoadedData = true
        };

        CrearSut().Apply(result, _log, SnapshotConCategoria("BDI", "A"));

        Assert.Empty(result.DatosPadronValidados);
        Assert.True(_log.HasWarnings);
    }
}
