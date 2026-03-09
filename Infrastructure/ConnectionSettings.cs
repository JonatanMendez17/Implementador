using System;
using ImplementadorCUAD.Services;

namespace ImplementadorCUAD.Infrastructure
{
    public static class ConnectionSettings
    {
        private static string? _cuadConnectionString;

        /// <summary>
        /// Connection string de la base CUAD (solo lectura).
        /// Se obtiene exclusivamente desde Configuracion.xml (sección Conexiones / nodo Cuad@connectionString).
        /// Si no está configurada, se lanza una excepción para bloquear el inicio de la aplicación.
        /// </summary>
        public static string CuadConnectionString
        {
            get
            {
                if (_cuadConnectionString != null)
                {
                    return _cuadConnectionString;
                }

                var fromConexiones = new ConexionesConfigService().GetCuadConnectionString();
                if (string.IsNullOrWhiteSpace(fromConexiones))
                {
                    throw new InvalidOperationException(
                        "No se encontró la cadena de conexión CUAD en Configuracion.xml. " +
                        "Revise el nodo <Conexiones><Cuad connectionString=\"...\" /></Conexiones> antes de iniciar la aplicación.");
                }

                _cuadConnectionString = fromConexiones;
                return _cuadConnectionString;
            }
        }
    }
}
