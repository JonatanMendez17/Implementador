using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;

namespace MigradorCUAD.Services
{
    public class ConfiguracionAppService
    {
        private readonly IConfiguration _configuration;

        public ConfiguracionAppService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            _configuration = builder.Build();
        }

        public string ObtenerConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection") ?? "";
        }
    }
}
