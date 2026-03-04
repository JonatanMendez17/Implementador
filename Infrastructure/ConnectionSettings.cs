using System;
using System.Configuration;

namespace ImplementadorCUAD.Infrastructure
{
    public static class ConnectionSettings
    {
        private const string DefaultConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=ImplementadorCUAD_DB;Trusted_Connection=True;TrustServerCertificate=True;";

        static ConnectionSettings()
        {
            var fromEnv = Environment.GetEnvironmentVariable("IMPLEMENTADORCUAD_CONNECTIONSTRING");
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                ConnectionString = fromEnv;
                return;
            }

            var fromConfig = ConfigurationManager.ConnectionStrings["ImplementadorCUADDb"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(fromConfig))
            {
                ConnectionString = fromConfig;
                return;
            }

            ConnectionString = DefaultConnectionString;
        }

        public static string ConnectionString { get; set; }
    }
}
