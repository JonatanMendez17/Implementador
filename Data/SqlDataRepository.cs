using Microsoft.Data.SqlClient;
using MigradorCUAD.Infrastructure;

namespace MigradorCUAD.Data
{
    public class SqlDataRepository : IDataRepository
    {
        public void TestConnection()
        {
            using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
            connection.Open();
        }
    }
}
