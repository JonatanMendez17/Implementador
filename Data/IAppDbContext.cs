using ImplementadorCUAD.Models;

namespace ImplementadorCUAD.Data;

public interface IAppDbContext : IDisposable
{
    void EnsureConnection();

    List<Entidad> GetEntidad();

    List<CategoriaCuadRef> GetCategoriasCuad();

    List<CatalogoServicioCuadRef> GetCatalogoServiciosCuad();

    HashSet<string> GetCategoriasConCuotaSocialVigente();

    HashSet<string> GetConceptosDescuentoVigentesParaConsumos();

    bool TryGetEmrIdByEmpleadoCodigoYDocumento(string empleadoCodigo, long documento, out int emrId);

    Dictionary<string, (bool Existe, int EmrId)> GetEmrIdByEmpleadoCodigoYDocumentoBatch(
        IEnumerable<(string EmpleadoCodigo, long Documento)> pares);

    Task<int> InsertPadronSocioAsync(IReadOnlyList<ImportarPadronSocio> registros, IProgress<int>? progress = null);

    Task<int> InsertImportarConsumosDetAsync(IReadOnlyList<ImportarConsumosDet> registros, IProgress<int>? progress = null);

    Task<int> InsertImportarConsumoCabAsync(IReadOnlyList<ImportarConsumoCab> registros, IProgress<int>? progress = null);

    bool ExistsImportedDataForEntidad(string entidad);

    (int Padron, int ConsumoCab, int ConsumoDet) DeleteImportedDataForEntidad(string entidadNombre, int entidadId);
}


