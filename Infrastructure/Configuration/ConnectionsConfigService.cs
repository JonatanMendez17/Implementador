using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Implementador.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Implementador.Infrastructure.Configuration
{
    /// Lee y actualiza la sección Conexiones de `Configuration.xml`.
    /// - `ConexionBase`: base (solo lectura).
    /// - `ConexionEmpleadores`: parámetros comunes para construir la conexión destino de cada empleador.
    public class ConnectionsConfigService
    {
        public const string RutaConfiguracionXml = "Configuration.xml";
        private readonly string _rutaXml = RutaConfiguracionXml;
        private readonly ILogger<ConnectionsConfigService>? _logger;

        public ConnectionsConfigService(ILogger<ConnectionsConfigService>? logger = null)
        {
            _logger = logger;
        }

        /// Obtiene el connection string de la base (`ConexionBase`).
        /// Devuelve null si el nodo no existe o no puede leerse.
        public string? GetConexionBaseConnectionString()
        {
            try
            {
                var document = XDocument.Load(_rutaXml);
                var conexiones = document.Root?.Element("Conexiones");
                var conexionBase = conexiones?.Element("ConexionBase");
                return conexionBase?.Attribute("connectionString")?.Value?.Trim();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Error leyendo {ConfigXmlPath} - metodo {Method}.",
                    _rutaXml, nameof(GetConexionBaseConnectionString));
                return null;
            }
        }

        /// Lista de empleadores definidos en configuración, con su connection string ya resuelto.
        public IReadOnlyList<EmpleadorConfig> GetEmpleadores()
        {
            var resultado = new List<EmpleadorConfig>();
            try
            {
                var document = XDocument.Load(_rutaXml);
                var conexiones = document.Root?.Element("Conexiones");
                if (conexiones == null)
                    return resultado;

                    var conexionEmpleadores = conexiones.Element("ConexionEmpleadores")
                        ?.Attribute("connectionString")?.Value?.Trim();
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
                    else if (!string.IsNullOrWhiteSpace(baseDatosAttr) && !string.IsNullOrWhiteSpace(conexionEmpleadores))
                    {
                        try
                        {
                            var builder = new SqlConnectionStringBuilder(conexionEmpleadores)
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
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Error leyendo {ConfigXmlPath} - metodo {Method}.",
                    _rutaXml, nameof(GetEmpleadores));
                return resultado;
            }
        }

        /// Actualiza la cadena de conexión de `ConexionBase` en `Configuration.xml`,
        /// agregando o modificando el nodo <Conexiones><ConexionBase connectionString="..." /></Conexiones>.
        /// Usa reemplazo de texto para preservar el formato original del archivo.
        public void SetConexionBaseConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(connectionString));

            var text = File.ReadAllText(_rutaXml);
            var escaped = EscapeXmlAttr(connectionString);

            // Caso 1: <ConexionBase connectionString="..."> ya existe → actualizar solo ese atributo
            const string patternConAttr = @"(<ConexionBase\b[^>]*?\bconnectionString\s*=\s*"")[^""]*("")";
            if (Regex.IsMatch(text, patternConAttr))
            {
                text = Regex.Replace(text, patternConAttr,
                    m => m.Groups[1].Value + escaped + m.Groups[2].Value);
                File.WriteAllText(_rutaXml, text);
                return;
            }

            // Caso 2: <ConexionBase> existe pero sin el atributo → agregarlo
            const string patternNodoSinAttr = @"(<ConexionBase\b)";
            if (Regex.IsMatch(text, patternNodoSinAttr))
            {
                text = Regex.Replace(text, patternNodoSinAttr,
                    m => $"{m.Groups[1].Value} connectionString=\"{escaped}\"");
                File.WriteAllText(_rutaXml, text);
                return;
            }

            // Caso 3: <ConexionBase> no existe → insertar dentro de <Conexiones>
            const string patternConexiones = @"(<Conexiones\b[^>]*>)";
            if (Regex.IsMatch(text, patternConexiones))
            {
                text = Regex.Replace(text, patternConexiones,
                    m => m.Groups[1].Value + $"\n    <ConexionBase connectionString=\"{escaped}\" />");
                File.WriteAllText(_rutaXml, text);
                return;
            }

            // Fallback: el XML no tiene estructura esperada → reescribir con XDocument
            var document = XDocument.Load(_rutaXml);
            var root = document.Root ?? new XElement("Configuracion");
            if (document.Root == null) document.Add(root);
            var conexiones = root.Element("Conexiones") ?? new XElement("Conexiones");
            if (root.Element("Conexiones") == null) root.Add(conexiones);
            var conexionBase = conexiones.Element("ConexionBase") ?? new XElement("ConexionBase");
            if (conexiones.Element("ConexionBase") == null) conexiones.AddFirst(conexionBase);
            conexionBase.SetAttributeValue("connectionString", connectionString);
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", OmitXmlDeclaration = true };
            using var writer = XmlWriter.Create(_rutaXml, settings);
            document.Save(writer);
        }

        private static string EscapeXmlAttr(string value) =>
            value.Replace("&", "&amp;")
                 .Replace("\"", "&quot;")
                 .Replace("<", "&lt;")
                 .Replace(">", "&gt;");
    }
}


