# Manual técnico (frontend) — Instalación y configuración

## Requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (LTS).
- Para compilar **Maui**: el workload correspondiente —
  `dotnet workload install maui` (o solo `maui-android` / `maui-windows` si
  no necesitas todas las plataformas). iOS y MacCatalyst requieren macOS con
  Xcode instalado — no se pueden compilar desde Windows/Linux.
- El backend (`sstcontrol-backend`) corriendo y accesible — ver su manual
  técnico de instalación.

## Configurar la URL de la API

**App Web** — `SstControl.Frontend.Web/wwwroot/appsettings.json` (desarrollo)
y `appsettings.Production.json` (producción):
```json
{ "ApiBaseUrl": "https://localhost:5080/" }
```
Reemplaza por la URL pública real de tu API antes de publicar.

**App Maui** — `SstControl.Frontend.Maui/MauiProgram.cs`, constantes
`UrlBaseApiAndroid` y `UrlBaseApiOtros`. El emulador de Android no puede
resolver `localhost` como la PC anfitriona — por eso usa el alias especial
`10.0.2.2`, que Android redirige automáticamente al `localhost` de quien lo
aloja. Para dispositivos físicos o producción, cambia ambas constantes a la
URL pública real de la API.

## Correr la app Web en desarrollo
```bash
cd SstControl.Frontend.Web
dotnet run
```
Por defecto en `https://localhost:5210` (puerto configurado en
`Properties/launchSettings.json`). Recuerda que esa URL debe estar en
`Cors:AllowedOrigins` del backend, o el navegador bloqueará las peticiones.

## Correr la app Maui en desarrollo
```bash
cd SstControl.Frontend.Maui
dotnet build -t:Run -f net10.0-windows10.0.19041.0   # Windows
dotnet build -t:Run -f net10.0-android                # Android (emulador o dispositivo)
dotnet build -t:Run -f net10.0-maccatalyst            # macOS
```

## Verificar que todo compila
```bash
dotnet restore
dotnet build
```
En el entorno donde se generó este proyecto no se pudo verificar la
restauración de paquetes NuGet (sin acceso de red al feed) — este es el
primer paso real a correr en tu máquina antes de dar por buena cualquier
versión de paquete fijada en los `.csproj`.
