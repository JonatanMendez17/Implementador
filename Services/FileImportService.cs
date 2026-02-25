using ExcelDataReader;
using MigradorCUAD.Data;
using MigradorCUAD.Models;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MigradorCUAD.Services
{
    public class FileImportService
    {
        public MigrationValidationResult ValidateAndLoadFiles(MigrationFileSelection selection, Action<string> log)
        {
            var result = new MigrationValidationResult();

            var datosCategorias = string.IsNullOrWhiteSpace(selection.ArchivoCategorias)
                ? null
                : LoadFile("Categorias", selection.ArchivoCategorias, log);

            var datosPadron = string.IsNullOrWhiteSpace(selection.ArchivoPadron)
                ? null
                : LoadFile("Padron", selection.ArchivoPadron, log);

            var datosConsumos = string.IsNullOrWhiteSpace(selection.ArchivoConsumos)
                ? null
                : LoadFile("Consumos", selection.ArchivoConsumos, log);

            var datosConsumosDetalle = string.IsNullOrWhiteSpace(selection.ArchivoConsumosDetalle)
                ? null
                : LoadFile("ConsumosDetalle", selection.ArchivoConsumosDetalle, log);

            var datosServicios = string.IsNullOrWhiteSpace(selection.ArchivoServicios)
                ? null
                : LoadFile("Servicios", selection.ArchivoServicios, log);

            var datosCatalogoServicios = string.IsNullOrWhiteSpace(selection.ArchivoCatalogoServicios)
                ? null
                : LoadFile("CatalogoServicios", selection.ArchivoCatalogoServicios, log);

            if (datosPadron != null)
            {
                result.DatosPadronValidados = datosPadron;
                result.HuboCarga = true;
            }

            if (datosCategorias != null)
            {
                result.DatosCategoriasValidadas = datosCategorias;
                result.HuboCarga = true;
            }

            if (datosConsumos != null)
            {
                result.DatosConsumosValidados = datosConsumos;
                result.HuboCarga = true;
            }

            if (datosConsumosDetalle != null)
            {
                result.DatosConsumosDetalleValidados = datosConsumosDetalle;
                result.HuboCarga = true;
            }

            if (datosCatalogoServicios != null)
            {
                result.DatosCatalogoServiciosValidados = datosCatalogoServicios;
                result.HuboCarga = true;
            }

            if (datosServicios != null)
            {
                result.DatosServiciosValidados = datosServicios;
                result.HuboCarga = true;
            }

            ApplyPadronSpecificValidations(result, log);
            ApplyConsumosSpecificValidations(result, log);

            if (result.HuboCarga)
            {
                log("Archivos cargados con validaciones generales.");
            }
            else
            {
                log("No se pudo cargar ningun archivo.");
            }

            return result;
        }

        private static void ApplyPadronSpecificValidations(MigrationValidationResult result, Action<string> log)
        {
            if (result.DatosPadronValidados.Count == 0)
            {
                return;
            }

            var categoriasValidasCodigo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var categoriasValidasNombre = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var filaCategoria in result.DatosCategoriasValidadas)
            {
                if (TryGetFirstValue(filaCategoria, out var codigo, "Codigo Categoria", "C骴igo Categor韆", "C贸digo Categor铆a") &&
                    !string.IsNullOrWhiteSpace(codigo))
                {
                    categoriasValidasCodigo.Add(codigo.Trim());
                }

                if (TryGetFirstValue(filaCategoria, out var nombre, "Categoria", "Categor韆", "Categor铆a") &&
                    !string.IsNullOrWhiteSpace(nombre))
                {
                    categoriasValidasNombre.Add(nombre.Trim());
                }
            }

            var padronFiltrado = new List<Dictionary<string, string>>();
            var sociosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var socioCategoria = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var documentosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var beneficiosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rechazadas = 0;

            for (int i = 0; i < result.DatosPadronValidados.Count; i++)
            {
                var fila = result.DatosPadronValidados[i];
                var numeroFila = i + 2;
                var filaValida = true;

                var nroSocio = GetFirstValue(fila, "Nro Socio");
                var codigoCategoria = GetFirstValue(fila, "Codigo Categoria", "C骴igo Categor韆", "C贸digo Categor铆a");
                var nombreCategoriaPadron = GetFirstValue(fila, "Categoria", "Categor韆", "Categor铆a");
                var documento = GetFirstValue(fila, "Documento");
                var beneficio = GetFirstValue(fila, "Beneficio");

                if (string.IsNullOrWhiteSpace(nroSocio))
                {
                    log($"ERROR Padron fila {numeroFila}: 'Nro Socio' vacio.");
                    filaValida = false;
                }
                else
                {
                    var nroSocioNormalizado = nroSocio.Trim();
                    if (!sociosVistos.Add(nroSocioNormalizado))
                    {
                        log($"ERROR Padron fila {numeroFila}: numero de socio '{nroSocio}' repetido.");
                        filaValida = false;
                    }

                    var categoriaNormalizada = (codigoCategoria ?? string.Empty).Trim();
                    if (socioCategoria.TryGetValue(nroSocioNormalizado, out var categoriaExistente) &&
                        !string.Equals(categoriaExistente, categoriaNormalizada, StringComparison.OrdinalIgnoreCase))
                    {
                        log($"ERROR Padron fila {numeroFila}: socio '{nroSocio}' afiliado a mas de una categoria.");
                        filaValida = false;
                    }
                    else
                    {
                        socioCategoria[nroSocioNormalizado] = categoriaNormalizada;
                    }
                }

                if (!IsCategoriaValida(codigoCategoria, nombreCategoriaPadron, categoriasValidasCodigo, categoriasValidasNombre))
                {
                    log($"ERROR Padron fila {numeroFila}: categoria informada no valida.");
                    filaValida = false;
                }

                if (!string.IsNullOrWhiteSpace(documento) && !documentosVistos.Add(documento.Trim()))
                {
                    log($"ERROR Padron fila {numeroFila}: documento '{documento}' repetido.");
                    filaValida = false;
                }

                if (!string.IsNullOrWhiteSpace(beneficio) && !beneficiosVistos.Add(beneficio.Trim()))
                {
                    log($"ERROR Padron fila {numeroFila}: beneficio '{beneficio}' repetido.");
                    filaValida = false;
                }

                if (filaValida)
                {
                    padronFiltrado.Add(fila);
                }
                else
                {
                    rechazadas++;
                }
            }

            if (rechazadas > 0)
            {
                log($"Resumen validacion especifica Padron: aceptadas={padronFiltrado.Count}, rechazadas={rechazadas}.");
            }

            result.DatosPadronValidados = padronFiltrado;
        }

        private static void ApplyConsumosSpecificValidations(MigrationValidationResult result, Action<string> log)
        {
            if (result.DatosConsumosValidados.Count == 0)
            {
                return;
            }

            HashSet<string> entidadesCuad;
            try
            {
                using var db = new AppDbContext();
                entidadesCuad = db.GetEntidades()
                    .SelectMany(e => new[]
                    {
                        e.Nombre?.Trim(),
                        e.EntId.ToString(CultureInfo.InvariantCulture)
                    })
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v!)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                log($"ERROR Consumos: no se pudo validar entidades de CUAD. {ex.Message}");
                result.DatosConsumosValidados = new List<Dictionary<string, string>>();
                return;
            }

            var padronPorSocio = result.DatosPadronValidados
                .Where(f => TryGetFirstValue(f, out var nro, "Nro Socio") && !string.IsNullOrWhiteSpace(nro))
                .GroupBy(f => GetFirstValue(f, "Nro Socio").Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var consumosFiltrados = new List<Dictionary<string, string>>();
            var codigosConsumoVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rechazadas = 0;

            for (int i = 0; i < result.DatosConsumosValidados.Count; i++)
            {
                var fila = result.DatosConsumosValidados[i];
                var numeroFila = i + 2;
                var filaValida = true;

                var entidad = GetFirstValue(fila, "Entidad");
                var nroSocio = GetFirstValue(fila, "Nro Socio");
                var cuitConsumo = GetFirstValue(fila, "CUIT");
                var beneficioConsumo = GetFirstValue(fila, "Beneficio");
                var codigoConsumo = GetFirstValue(fila, "Codigo", "C骴igo", "C贸digo");

                if (string.IsNullOrWhiteSpace(entidad) || !entidadesCuad.Contains(entidad.Trim()))
                {
                    log($"ERROR Consumos fila {numeroFila}: entidad '{entidad}' no existe en CUAD.");
                    filaValida = false;
                }

                if (string.IsNullOrWhiteSpace(nroSocio) || !padronPorSocio.TryGetValue(nroSocio.Trim(), out var filaPadron))
                {
                    log($"ERROR Consumos fila {numeroFila}: socio '{nroSocio}' no existe o no corresponde al padron.");
                    filaValida = false;
                }
                else
                {
                    var cuitPadron = GetFirstValue(filaPadron, "CUIT");
                    var beneficioPadron = GetFirstValue(filaPadron, "Beneficio");

                    if (!EqualsDigitsOnly(cuitConsumo, cuitPadron))
                    {
                        log($"ERROR Consumos fila {numeroFila}: CUIT no coincide con padron para socio '{nroSocio}'.");
                        filaValida = false;
                    }

                    if (!EqualsTrimmed(beneficioConsumo, beneficioPadron))
                    {
                        log($"ERROR Consumos fila {numeroFila}: Beneficio no coincide con padron para socio '{nroSocio}'.");
                        filaValida = false;
                    }
                }

                if (string.IsNullOrWhiteSpace(codigoConsumo))
                {
                    log($"ERROR Consumos fila {numeroFila}: codigo de consumo vacio.");
                    filaValida = false;
                }
                else if (!codigosConsumoVistos.Add(codigoConsumo.Trim()))
                {
                    log($"ERROR Consumos fila {numeroFila}: codigo de consumo '{codigoConsumo}' repetido.");
                    filaValida = false;
                }

                if (filaValida)
                {
                    consumosFiltrados.Add(fila);
                }
                else
                {
                    rechazadas++;
                }
            }

            if (rechazadas > 0)
            {
                log($"Resumen validacion especifica Consumos: aceptadas={consumosFiltrados.Count}, rechazadas={rechazadas}.");
            }

            result.DatosConsumosValidados = consumosFiltrados;
        }

        private static bool IsCategoriaValida(
            string? codigoCategoria,
            string? nombreCategoriaPadron,
            HashSet<string> categoriasValidasCodigo,
            HashSet<string> categoriasValidasNombre)
        {
            if (categoriasValidasCodigo.Count == 0 && categoriasValidasNombre.Count == 0)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(codigoCategoria) && categoriasValidasCodigo.Contains(codigoCategoria.Trim()))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(nombreCategoriaPadron) && categoriasValidasNombre.Contains(nombreCategoriaPadron.Trim()))
            {
                return true;
            }

            return false;
        }

        private string[] ReadFileLines(string rutaArchivo)
        {
            var extension = Path.GetExtension(rutaArchivo).ToLowerInvariant();

            if (extension == ".csv" || extension == ".txt")
            {
                return File.ReadAllLines(rutaArchivo);
            }

            if (extension == ".xls" || extension == ".xlsx")
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var filas = new List<string>();
                var builder = new StringBuilder();

                using var stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                do
                {
                    while (reader.Read())
                    {
                        builder.Clear();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (i > 0)
                            {
                                builder.Append(',');
                            }

                            var valor = reader.GetValue(i)?.ToString() ?? string.Empty;
                            builder.Append(valor);
                        }

                        filas.Add(builder.ToString());
                    }
                } while (reader.NextResult());

                return filas.ToArray();
            }

            return File.ReadAllLines(rutaArchivo);
        }

        private List<Dictionary<string, string>>? LoadFile(string nombreLogico, string? rutaArchivo, Action<string> log)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rutaArchivo))
                {
                    return null;
                }

                if (!File.Exists(rutaArchivo))
                {
                    log($"Archivo invalido: {nombreLogico}");
                    return null;
                }

                var configService = new ConfiguracionService();
                var columnasConfig = configService.ObtenerColumnas(nombreLogico);
                if (columnasConfig.Count == 0)
                {
                    log($"No existe configuracion XML para {nombreLogico}");
                    return null;
                }

                var lineas = ReadFileLines(rutaArchivo);
                if (lineas.Length == 0)
                {
                    log($"Archivo {nombreLogico} vacio.");
                    return null;
                }

                // Modo prueba: validaciones estructurales deshabilitadas.
                //var encabezado = lineas[0].Split(',');
                //if (encabezado.Length != columnasConfig.Count) return null;
                //for (int i = 0; i < encabezado.Length; i++)
                //{
                //    if (encabezado[i] != columnasConfig[i].Nombre) return null;
                //}

                var registros = new List<Dictionary<string, string>>();
                var clavesUnicas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var totalFilasDatos = Math.Max(0, lineas.Length - 1);
                var filasAceptadas = 0;
                var filasRechazadas = 0;

                for (int i = 1; i < lineas.Length; i++)
                {
                    var valores = lineas[i].Split(',');
                    var fila = new Dictionary<string, string>();
                    var filaEsValida = true;

                    for (int j = 0; j < columnasConfig.Count; j++)
                    {
                        var valor = j < valores.Length ? valores[j] : string.Empty;
                        var config = columnasConfig[j];

                        // Modo prueba: validacion de tipo deshabilitada.
                        //var config = columnasConfig[j];
                        //if (!ValidateDataType(valor, config)) { ... }

                        if (!ValidateGeneralRules(valor, config, out var error))
                        {
                            log($"ERROR {nombreLogico} fila {i + 1}, columna '{config.Nombre}': {error}");
                            filaEsValida = false;
                        }

                        fila[config.Nombre] = valor;
                    }

                    if (filaEsValida)
                    {
                        if (ValidateSpecificUniqueness(nombreLogico, i + 1, fila, clavesUnicas, log))
                        {
                            registros.Add(fila);
                            filasAceptadas++;
                        }
                        else
                        {
                            filasRechazadas++;
                        }
                    }
                    else
                    {
                        filasRechazadas++;
                    }
                }

                log($"{nombreLogico} cargado con validaciones generales.");
                log($"Resumen {nombreLogico}: total={totalFilasDatos}, aceptadas={filasAceptadas}, rechazadas={filasRechazadas}.");
                return registros;
            }
            catch (Exception ex)
            {
                log($"Error al cargar {nombreLogico}: {ex.Message}");
                return null;
            }
        }

        private static bool ValidateDataType(string valor, ColumnaConfiguracion config)
        {
            if (valor.Length > config.LargoMaximo)
            {
                return false;
            }

            switch (config.TipoDato.ToLowerInvariant())
            {
                case "int":
                    return int.TryParse(valor, out _);
                case "decimal":
                    return decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
                case "fecha":
                    return DateTime.TryParse(valor, out _);
                case "texto":
                    return !string.IsNullOrWhiteSpace(valor);
                default:
                    return false;
            }
        }

        private static bool ValidateGeneralRules(string valor, ColumnaConfiguracion config, out string error)
        {
            error = string.Empty;
            var texto = valor?.Trim() ?? string.Empty;

            if (texto.Length == 0)
            {
                return true;
            }

            if (texto.Length > config.LargoMaximo)
            {
                error = $"supera el largo maximo permitido ({config.LargoMaximo})";
                return false;
            }

            if (HasWeirdCharacters(texto))
            {
                error = "contiene caracteres extranos";
                return false;
            }

            switch (config.TipoDato.ToLowerInvariant())
            {
                case "int":
                    if (!int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
                    {
                        error = "debe ser un numero entero sin letras";
                        return false;
                    }

                    if (numero <= 0)
                    {
                        error = "debe ser un numero entero positivo";
                        return false;
                    }

                    return true;

                case "decimal":
                    if (!TryParseDecimalFlexible(texto, out _))
                    {
                        error = "debe ser un valor de dinero valido";
                        return false;
                    }

                    return true;

                case "fecha":
                    if (!DateTime.TryParse(texto, CultureInfo.GetCultureInfo("es-AR"), DateTimeStyles.None, out _) &&
                        !DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    {
                        error = "debe ser una fecha valida";
                        return false;
                    }

                    return true;

                case "texto":
                    return true;

                default:
                    error = $"tipo de dato no soportado: {config.TipoDato}";
                    return false;
            }
        }

        private static bool TryParseDecimalFlexible(string texto, out decimal valor)
        {
            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor) ||
                   decimal.TryParse(texto, NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out valor);
        }

        private static bool HasWeirdCharacters(string texto)
        {
            return Regex.IsMatch(texto, @"[^\p{L}\p{N}\s\.\,\;\:\-\/\\\(\)\'\""\#\%\&\+]");
        }

        private static bool ValidateSpecificUniqueness(
            string nombreLogico,
            int numeroFila,
            Dictionary<string, string> fila,
            HashSet<string> clavesUnicas,
            Action<string> log)
        {
            if (nombreLogico.Equals("Padron", StringComparison.OrdinalIgnoreCase))
            {
                var nroSocio = GetFirstValue(fila, "Nro Socio");
                if (string.IsNullOrWhiteSpace(nroSocio))
                {
                    log($"ERROR Padron fila {numeroFila}: 'Nro Socio' vacio.");
                    return false;
                }

                var clave = $"PADRON::{nroSocio.Trim()}";
                if (!clavesUnicas.Add(clave))
                {
                    log($"ERROR Padron fila {numeroFila}: numero de socio '{nroSocio}' repetido.");
                    return false;
                }

                return true;
            }

            if (nombreLogico.Equals("Consumos", StringComparison.OrdinalIgnoreCase))
            {
                var nroConsumo = GetFirstValue(fila, "Codigo", "C骴igo", "C贸digo");
                if (string.IsNullOrWhiteSpace(nroConsumo))
                {
                    log($"ERROR Consumos fila {numeroFila}: codigo (nro de consumo) vacio.");
                    return false;
                }

                var clave = $"CONSUMOS::{nroConsumo.Trim()}";
                if (!clavesUnicas.Add(clave))
                {
                    log($"ERROR Consumos fila {numeroFila}: nro de consumo '{nroConsumo}' repetido.");
                    return false;
                }

                return true;
            }

            return true;
        }

        private static bool EqualsTrimmed(string? left, string? right)
        {
            var a = (left ?? string.Empty).Trim();
            var b = (right ?? string.Empty).Trim();
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EqualsDigitsOnly(string? left, string? right)
        {
            static string Digits(string? text) => new string((text ?? string.Empty).Where(char.IsDigit).ToArray());
            return string.Equals(Digits(left), Digits(right), StringComparison.Ordinal);
        }

        private static bool TryGetFirstValue(Dictionary<string, string> fila, out string value, params string[] posiblesClaves)
        {
            foreach (var clave in posiblesClaves)
            {
                if (fila.TryGetValue(clave, out var encontrado))
                {
                    value = encontrado;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        private static string GetFirstValue(Dictionary<string, string> fila, params string[] posiblesClaves)
        {
            return TryGetFirstValue(fila, out var value, posiblesClaves) ? value : string.Empty;
        }
    }
}
