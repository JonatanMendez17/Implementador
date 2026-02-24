using Microsoft.Data.SqlClient;

namespace MigradorCUAD.Services
{
    public class DatabaseService(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public bool ProbarConexion()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection.State == System.Data.ConnectionState.Open;
        }
    }
}
