# Implementador CUAD

Aplicación de escritorio WPF (.NET 8) orientada a la importación, validación e implementación de información CUAD en SQL Server.

## Que es la aplicacion y cual es su proposito

Implementador CUAD centraliza el proceso operativo de carga de archivos para cada empleador, reduciendo errores manuales y asegurando consistencia antes de persistir datos.

La app trabaja con dos tipos de origen/destino:

- **Base de referencia (solo lectura):** provee catálogos y entidades maestras.
- **Base por empleador (escritura):** recibe los datos importados luego de validar.

Su objetivo principal es garantizar que los archivos cumplan reglas de estructura y negocio antes de insertar datos productivos.

## Cual es su arquitectura

La solucion sigue una arquitectura por capas para separar responsabilidades:

- **`Presentation/`**: vistas WPF (`MainWindow`) y componentes visuales.
- **`ViewModels/`**: coordinacion de flujo de UI y comandos (patron MVVM).
- **`Application/`**: logica de negocio, importacion, validaciones y orquestacion de implementacion.
- **`Data/`**: acceso a datos con `DbContext`, contratos y persistencia.
- **`Infrastructure/`**: fabricas, servicios de soporte y detalles tecnicos transversales.
- **`Models/`**: modelos de dominio y DTOs usados entre capas.
- **`Commands/`**: implementaciones de comandos para la capa de presentacion.

Esta separacion permite evolucionar reglas de negocio sin acoplarlas a la UI o al acceso a datos.

## Cosas mas destacadas de la aplicacion

- Importa y procesa 6 tipos de archivo CUAD: categorias, padron, consumos, consumos detalle, servicios y catalogo de servicios.
- Aplica validaciones previas de estructura y reglas de negocio antes de implementar datos.
- Soporta multiples empleadores con configuracion centralizada en `Configuration.xml`.
- Permite limpiar datos por entidad en la base destino cuando se requiere reprocesar.
- Muestra version en pantalla para facilitar trazabilidad operativa.

## Requisitos

- .NET SDK 8.0 o superior
- SQL Server accesible desde la maquina donde corre la aplicacion
- Configuracion de conexiones en `Configuration.xml` (seccion `<Conexiones>`)

## Ejecucion

1. Abrir la solucion `Implementador.sln` en Visual Studio.
2. Restaurar paquetes NuGet y compilar la solucion.
3. Configurar `Configuration.xml` con la base y empleadores.
4. Ejecutar el proyecto `Implementador` como proyecto de inicio.

## Configuracion

### Conexiones a bases de datos (`Configuration.xml`)

En la seccion `<Conexiones>` se definen:

- **`<ConexionBase>`**: connection string de la base de referencia (solo lectura).
- **`<ConexionEmpleadores>`**: servidor y autenticacion comunes para las bases destino.
- **`<Empleador>`**: empleadores disponibles en UI (`nombre` y `baseDatos`).

Tambien se puede indicar un connection string completo por empleador con el atributo `connectionString` en lugar de `baseDatos`.
Si no existe la seccion `<Conexiones>` o no hay empleadores configurados, no se podra implementar ni limpiar datos.

### Columnas de los archivos (`Configuration.xml`)

Para cada tipo de archivo se pueden parametrizar:

- columnas esperadas
- alias de columnas
- tipo de dato
- largo maximo
- obligatoriedad

Si una columna no aplica para un flujo, puede comentarse en configuracion para dejar de exigirla.

## Uso de la aplicacion

1. Seleccionar **Empleador** y **Entidad**.
2. Cargar los archivos requeridos segun el proceso.
3. Pulsar **Validar** para verificar consistencia y precondiciones.
4. Pulsar **Implementar datos** para persistir en la base del empleador.
5. Opcional: **Limpiar entidad** para borrar la informacion importada de esa entidad.

## Versionado

La version se define en `Implementador.csproj` (`Version`, `AssemblyVersion`, `FileVersion`).
Al publicar una nueva version, actualizar esos campos (por ejemplo `1.0.0` -> `1.1.0`).
