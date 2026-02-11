using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MigradorCUAD.Models;

namespace MigradorCUAD.Services
{
    /// <summary>
    /// Servicio encargado de leer la configuración de columnas
    /// desde el archivo XML de migración.
    /// </summary>
    public class ConfiguracionService
    {
        private readonly string _rutaXml = "ConfiguracionMigracion.xml";

        /// <summary>
        /// Obtiene la lista de columnas configuradas para un archivo lógico.
        /// </summary>
        /// <param name="nombreArchivo">Nombre lógico del archivo (por ejemplo, "Categorias").</param>
        /// <returns>Lista de columnas configuradas.</returns>
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
                // Se delega el detalle del error al llamador (por ejemplo, MainViewModel)
                // para que pueda registrarlo en la interfaz de usuario.
                throw;
            }
        }
    }
}
