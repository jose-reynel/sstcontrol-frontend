# Manual técnico (frontend) — Despliegue Web y Mobile

## Web (Blazor WebAssembly) — servidor de aplicaciones propio

La app Web publicada es un conjunto de archivos **100% estáticos** (HTML,
JS, WASM, CSS) — no necesita un runtime .NET corriendo en el servidor donde
se aloja, solo un servidor HTTP capaz de servir archivos estáticos con
manejo de rutas de SPA.

### 1. Publicar
```bash
cd SstControl.Frontend.Web
dotnet publish -c Release -o ./publicado
```
El resultado usable está en `./publicado/wwwroot`.

### 2. Ajustar `ApiBaseUrl` para producción
Antes de publicar, confirma que `wwwroot/appsettings.Production.json` tenga
la URL real de tu backend (ver `02-instalacion-y-configuracion.md`) — el
build la incluye tal cual, como archivo estático.

### 3. Servirlo en tu propio servidor de aplicaciones
Sirve `./publicado/wwwroot` con cualquier servidor de archivos estáticos
(Nginx, Apache, IIS, un contenedor Nginx propio). Dos configuraciones que
**sí son obligatorias** para que una SPA de Blazor funcione:

- **Fallback de SPA**: cualquier ruta que no sea un archivo real (por
  ejemplo `/documentos` al refrescar el navegador) debe servir
  `index.html` — si no, da 404 al refrescar cualquier página que no sea la
  raíz.
- **Tipo MIME `.wasm`**: algunos servidores no lo reconocen por defecto
  (`application/wasm`) — sin esto, el navegador rechaza cargar la app.

Ejemplo mínimo de Nginx:
```nginx
server {
    listen 80;
    root /var/www/sstcontrol-web;
    location / { try_files $uri $uri/ /index.html; }
    types { application/wasm wasm; }
}
```

### 4. O publícalo en GitHub Pages (ya automatizado)
`.github/workflows/deploy-web.yml` compila y publica automáticamente en
cada push a `main`. Actívalo una vez en **Settings → Pages → Source: GitHub
Actions**. El workflow ya ajusta el `<base href>` para una Pages de
proyecto y agrega el `404.html` de respaldo (mismo problema de fallback de
SPA, resuelto para el caso específico de GitHub Pages). Recuerda que el
dominio de Pages debe estar en `Cors:AllowedOrigins` del backend.

## Mobile / Desktop (.NET MAUI) — distribución en tiendas o instalador directo

A diferencia de la Web, estas builds **sí requieren recompilar** para cada
plataforma de destino (no hay "un solo artefacto" universal).

### Android
```bash
dotnet publish SstControl.Frontend.Maui -f net10.0-android -c Release \
  /p:AndroidPackageFormat=apk    # o "aab" para subir a Google Play
```
Firma el `.apk`/`.aab` con tu keystore antes de distribuirlo — ver la
[documentación oficial de firma de apps .NET MAUI](https://learn.microsoft.com/dotnet/maui/android/deployment/publish-cli)
para el flujo completo (no específico de este proyecto).

### Windows
```bash
dotnet publish SstControl.Frontend.Maui -f net10.0-windows10.0.19041.0 -c Release
```
Genera un paquete MSIX (ver `Platforms/Windows/Package.appxmanifest`) — para
distribución fuera de la Microsoft Store, necesitas firmarlo con un
certificado de confianza o distribuirlo como instalación *sideload*.

### iOS / macOS (Catalyst)
Requieren una Mac con Xcode y una cuenta de Apple Developer para firmar y
publicar en la App Store — no es posible completarlo desde este entorno de
desarrollo; sigue la
[guía oficial de publicación iOS de .NET MAUI](https://learn.microsoft.com/dotnet/maui/ios/deployment/).

### Antes de publicar cualquier build de dispositivo
Confirma que `MauiProgram.cs` apunte a la URL pública real de la API (no a
`localhost`/`10.0.2.2`, que solo sirven para desarrollo local) — ver
`02-instalacion-y-configuracion.md`.
