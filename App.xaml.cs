using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using ImplementadorCUAD.Data;
using ImplementadorCUAD.Services;

namespace ImplementadorCUAD
{
    public partial class App : Application
    {
        public App()
        {
            // Manejo global de errores no controlados
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using var db = new AppDbContext();
                db.EnsureConnection();
            }
            catch (SqlException ex)
            {
                DialogService.Show(
                    "No se pudo conectar a la base de datos configurada.\n\n" +
                    "Revise en el archivo Configuracion.xml que:\n" +
                    "  • El servidor SQL existe y está encendido.\n" +
                    "  • El nombre de la base de datos es correcto.\n" +
                    "  • El usuario y la contraseña son válidos.\n\n" +
                    $"Detalle técnico (para soporte):\n{ex.Message}",
                    "Error de conexión a base de datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Cerrar la aplicación porque el inicio falló
                Shutdown(-1);
            }
            catch (Exception ex)
            {
                DialogService.Show(
                    $"Se produjo un error al iniciar la aplicación.\n\nMensaje: {ex.Message}",
                    "Error al iniciar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Cerrar la aplicación porque el inicio falló
                Shutdown(-1);
            }
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DialogService.Show(
                $"Se produjo un error inesperado en la interfaz.\n\nMensaje: {e.Exception.Message}",
                "Error de aplicación",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Marcamos como manejado para evitar que WPF cierre la app de forma silenciosa
            e.Handled = true;
        }

        private static void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                DialogService.Show(
                    $"Se produjo un error crítico.\n\nMensaje: {ex.Message}",
                    "Error crítico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            DialogService.Show(
                $"Se produjo un error en una tarea en segundo plano.\n\nMensaje: {e.Exception.Message}",
                "Error en tarea",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.SetObserved();
        }
    }
}
