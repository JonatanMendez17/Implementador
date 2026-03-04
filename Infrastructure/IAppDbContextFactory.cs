using ImplementadorCUAD.Data;

namespace ImplementadorCUAD.Infrastructure;

public interface IAppDbContextFactory
{
    IAppDbContext Create();
}

public sealed class AppDbContextFactory : IAppDbContextFactory
{
    public IAppDbContext Create()
    {
        return new AppDbContext();
    }
}

