using MigradorCUAD.Models;

namespace MigradorCUAD.Services
{
    public static class CrossValidator
    {
        public static List<string> Validate(
            List<ImportarPadronSocio> socios,
            List<ImportarConsumoCab> consumos,
            List<ImportarConsumosDet> detalles)
            //List<Servicio> servicios)
        {
            var errores = new List<string>();

            errores.AddRange(ValidarConsumos(socios, consumos));
            errores.AddRange(ValidarDetalles(consumos, detalles));
            //errores.AddRange(ValidarServicios(socios, servicios));

            return errores;
        }

        private static List<string> ValidarConsumos(
            List<ImportarPadronSocio> socios,
            List<ImportarConsumoCab> consumos)
         {
            var errores = new List<string>();

            // Duplicados
            var duplicados = consumos
                .GroupBy(c => c.CodigoConsumo)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var nro in duplicados)
                errores.Add($"Consumo duplicado: {nro}");

            // Socio inexistente
            foreach (var consumo in consumos)
            {
                if (!socios.Any(s => s.NroSocio == consumo.NroSocio))
                {
                    errores.Add($"El socio {consumo.NroSocio} no existe para el consumo {consumo.NroSocio}");
                }
            }

            return errores;
        }

        private static List<string> ValidarDetalles(
            List<ImportarConsumoCab> consumos,
            List<ImportarConsumosDet> detalles)
        {
            var errores = new List<string>();

            foreach (var detalle in detalles)
            {
                if (!consumos.Any(c => c.CodigoConsumo == detalle.CodigoConsumo))
                {
                    errores.Add($"Detalle con consumo inexistente: {detalle.CodigoConsumo}");
                }

                if (detalle.FechaVencimiento < DateTime.Today)
                {
                    errores.Add($"Primer vencimiento inválido en consumo {detalle.CodigoConsumo}");
                }
            }

            return errores;
        }
    }
}
