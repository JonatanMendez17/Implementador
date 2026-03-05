using ImplementadorCUAD.Data;

namespace ImplementadorCUAD.Infrastructure;

public interface IAppDbContextFactory
{
    /// <summary>
    /// Crea un contexto contra la base CUAD (lecturas de referencia: Entidad, CategoriasCuad, etc.).
    /// </summary>
    IAppDbContext Create();

    /// <summary>
    /// Crea un contexto contra la base indicada (destino del empleador para importación/limpieza).
    /// Si targetConnectionString es null o vacío, usa la conexión CUAD (compatibilidad).
    /// </summary>
    IAppDbContext Create(string? targetConnectionString);
}

public sealed class AppDbContextFactory : IAppDbContextFactory
{
    public IAppDbContext Create()
    {
        return new AppDbContext();
    }

    public IAppDbContext Create(string? targetConnectionString)
    {
        return new AppDbContext(targetConnectionString ?? string.Empty);
    }
}

