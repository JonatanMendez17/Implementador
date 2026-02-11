using Microsoft.Data.SqlClient;

namespace MigradorCUAD.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool ProbarConexion()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection.State == System.Data.ConnectionState.Open;
        }
    }
}
