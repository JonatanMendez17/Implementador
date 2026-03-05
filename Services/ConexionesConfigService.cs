using System.Xml.Linq;
using ImplementadorCUAD.Models;
using Microsoft.Data.SqlClient;

namespace ImplementadorCUAD.Services
{
    /// <summary>
    /// Lee la sección Conexiones de Configuracion.xml: conexión CUAD y lista de empleadores con su connection string.
    /// </summary>
    public class ConexionesConfigService
    {
        private readonly string _rutaXml = "Configuracion.xml";

        /// <summary>
        /// Obtiene el connection string de la base CUAD (solo lectura). Devuelve null si no existe la sección.
        /// </summary>
        public string? GetCuadConnectionString()
        {
            try
            {
                var document = XDocument.Load(_rutaXml);
                var conexiones = document.Root?.Element("Conexiones");
                var cuad = conexiones?.Element("Cuad");
                return cuad?.Attribute("connectionString")?.Value?.Trim();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Lista de empleadores definidos en configuración, con su connection string ya resuelto.
        /// Si no existe la sección Conexiones o no hay Empleador, devuelve lista vacía.
        /// </summary>
        public IReadOnlyList<EmpleadorConfig> GetEmpleadores()
        {
            var resultado = new List<EmpleadorConfig>();
            try
            {
                var document = XDocument.Load(_rutaXml);
                var conexiones = document.Root?.Element("Conexiones");
                if (conexiones == null)
                    return resultado;

                var conexionBase = conexiones.Element("ConexionBase")?.Attribute("connectionString")?.Value?.Trim();
                var empleadorElements = conexiones.Elements("Empleador").ToList();

                foreach (var emp in empleadorElements)
                {
                    var nombre = emp.Attribute("nombre")?.Value?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(nombre))
                        continue;

                    var connectionStringAttr = emp.Attribute("connectionString")?.Value?.Trim();
                    var baseDatosAttr = emp.Attribute("baseDatos")?.Value?.Trim();

                    string? connectionString = null;
                    if (!string.IsNullOrWhiteSpace(connectionStringAttr))
                    {
                        connectionString = connectionStringAttr;
                    }
                    else if (!string.IsNullOrWhiteSpace(baseDatosAttr) && !string.IsNullOrWhiteSpace(conexionBase))
                    {
                        try
                        {
                            var builder = new SqlConnectionStringBuilder(conexionBase)
                            {
                                InitialCatalog = baseDatosAttr
                            };
                            connectionString = builder.ConnectionString;
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(connectionString))
                        continue;

                    resultado.Add(new EmpleadorConfig
                    {
                        Nombre = nombre,
                        ConnectionString = connectionString,
                        BaseDatos = baseDatosAttr
                    });
                }

                return resultado;
            }
            catch
            {
                return resultado;
            }
        }
    }
}
