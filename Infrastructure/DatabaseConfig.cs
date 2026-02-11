using System.Configuration;

namespace MigradorCUAD.Infrastructure
{
    public static class DatabaseConfig
    {
        public static string? ConnectionString =>
            ConfigurationManager.ConnectionStrings["MigradorCUADDb"]?.ConnectionString;
    }
}
