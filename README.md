# ImplementadorCUAD

Aplicación de escritorio WPF (.NET 8) para importar archivos de CUAD (padrones, consumos, servicios, etc.) y cargarlos en SQL Server.

## Requisitos

- .NET SDK 8.0 o superior.
- SQL Server accesible desde la máquina donde corre la aplicación.
- Cadena de conexión configurada en `App.config` (sección `connectionStrings`) o por variable de entorno.

## Ejecución

1. Abrir la solución `ImplementadorCUAD.sln` en Visual Studio o en un IDE compatible.
2. Restaurar paquetes NuGet y compilar la solución.
3. Verificar que la cadena de conexión apunte a una base válida.
4. Ejecutar el proyecto `ImplementadorCUAD` como proyecto de inicio.

## Configuración de la conexión a base de datos

La aplicación obtiene la cadena de conexión de la siguiente forma (en este orden):

1. Variable de entorno `IMPLEMENTADORCUAD_CONNECTIONSTRING` (si está definida).
2. Entrada `connectionStrings` con nombre `ImplementadorCUADDb` en `App.config`.
3. Valor por defecto embebido en `ConnectionSettings`.

Para cambiar de base de datos, lo más sencillo es:

- Editar `App.config` y ajustar la cadena `ImplementadorCUADDb`, o
- Definir `IMPLEMENTADORCUAD_CONNECTIONSTRING` apuntando al servidor/base deseados.
