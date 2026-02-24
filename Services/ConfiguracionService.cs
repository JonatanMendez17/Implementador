using System.Xml.Linq;
using MigradorCUAD.Models;

namespace MigradorCUAD.Services
{
    /// Servicio encargado de leer la configuración de columnas
    public class ConfiguracionService
    {
        private readonly string _rutaXml = "ConfiguracionMigracion.xml";

        /// Obtiene la lista de columnas configuradas para un archivo lógico.
        public List<ColumnaConfiguracion> ObtenerColumnas(string nombreArchivo)
        {
            try
            {
                var document = XDocument.Load(_rutaXml);

                var columnas = document
                    .Descendants("Archivo")
                    .Where(a => a.Attribute("nombre")?.Value == nombreArchivo)
                    .Descendants("Columna")
                    .Select(c => new ColumnaConfiguracion
                    {
                        Nombre = c.Attribute("nombre")?.Value ?? string.Empty,
                        TipoDato = c.Attribute("tipo")?.Value ?? string.Empty,
                        LargoMaximo = int.Parse(c.Attribute("largoMaximo")?.Value ?? "0")
                    })
                    .ToList();

                return columnas;
            }
            catch (Exception)
            {
                // Se delega el detalle del error al llamador para que pueda registrarlo en la interfaz de usuario.
                throw;
            }
        }
    }
}
