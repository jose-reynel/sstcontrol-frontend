# SstControl — Frontend

Aplicación de administración para **SstControl** (Seguridad y Salud en el
Trabajo), compatible con el backend publicado en
[`sstcontrol-backend`](https://github.com/jose-reynel/sstcontrol-backend).

Una sola interfaz (Blazor, .NET 10 LTS) compilada para **dos destinos**:

- 🌐 **Web** — Blazor WebAssembly (PWA, instalable, funciona offline con lo ya cacheado).
- 📱 **Mobile / Desktop** — .NET MAUI Blazor Hybrid: Android, iOS, macOS (Catalyst) y Windows, desde el mismo código.

## Estructura de la solución

```
SstControl.Frontend.slnx
├── SstControl.Frontend.Shared   ← Librería Razor compartida: TODA la UI vive acá
│   ├── Models/                  ← DTOs espejo de SstControl.Aplicacion.DTOs (backend)
│   ├── Services/                ← Cliente HTTP tipado, autenticación JWT, RBAC en cliente
│   ├── Layout/                  ← MainLayout, NavMenu
│   ├── Pages/                   ← Panel, Documentos, Actas, Empresas, Administración (RBAC)
│   ├── Components/              ← Componentes reutilizables (p. ej. insignia de estado)
│   ├── Routes.razor             ← Router único, usado por Web y por Maui
│   └── wwwroot/css/app.css      ← Sistema de diseño compartido
├── SstControl.Frontend.Web      ← Host Blazor WebAssembly (PWA)
└── SstControl.Frontend.Maui     ← Host .NET MAUI Blazor Hybrid (Android/iOS/MacCatalyst/Windows)
```

La idea central: **una sola vez** se escribe cada página, cada llamada a la
API y cada regla de permisos, en `SstControl.Frontend.Shared`. Los dos
proyectos "host" (`Web` y `Maui`) solo aportan lo que es específico de la
plataforma: cómo se guarda el token de sesión (`ITokenStore`) y la ventana o
página HTML donde se monta Blazor.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (LTS).
- Para compilar la app **Maui** además hace falta el workload correspondiente:

  ```bash
  dotnet workload install maui
  ```

  (Para compilar solo para Android/Windows se puede instalar únicamente
  `maui-android` / `maui-windows`; iOS y MacCatalyst requieren macOS con Xcode.)

## Cómo correr la app Web

```bash
cd SstControl.Frontend.Web
dotnet run
```

Por defecto apunta a `https://localhost:5080/` (ver
`wwwroot/appsettings.json`, clave `ApiBaseUrl`) — debe coincidir con la URL
donde esté corriendo `SstControl.Api` del backend. Para producción, edita
`wwwroot/appsettings.Production.json` con la URL pública real de la API.

## Cómo correr la app Maui

```bash
cd SstControl.Frontend.Maui
dotnet build -t:Run -f net10.0-windows10.0.19041.0   # Windows
dotnet build -t:Run -f net10.0-android                # Android (emulador/dispositivo)
dotnet build -t:Run -f net10.0-maccatalyst            # macOS
```

La URL de la API para las builds nativas está en
`MauiProgram.cs` (`UrlBaseApiAndroid` / `UrlBaseApiOtros`) — en el emulador
Android, `10.0.2.2` apunta al `localhost` de la PC anfitriona.

## Despliegue de la Web en GitHub Pages

El workflow `.github/workflows/deploy-web.yml` publica automáticamente la
app Web en GitHub Pages en cada push a `main`. Actívalo una vez en
**Settings → Pages → Source: GitHub Actions**. Recuerda que el backend debe
tener habilitado el origen de Pages en su configuración de CORS (ver
`Cors:AllowedOrigin` en `appsettings.json` del backend).

## Autenticación y permisos

El backend expone `POST /api/autenticacion/iniciar-sesion`, que devuelve un
JWT con los roles y permisos efectivos del usuario como claims. El frontend:

1. Guarda el token (`localStorage` en Web, almacenamiento seguro del SO en Maui).
2. Lo decodifica en el cliente (`DecodificadorJwt`) para saber quién es el
   usuario sin otra llamada a la API.
3. Muestra/oculta acciones según el mismo permiso que exige la API
   (`ExtensionesClaimsPrincipal.TienePermiso`, espejo de
   `SstControl.Api.Seguridad.ExtensionesPermisos` del backend) — la
   autorización real, por supuesto, la sigue validando siempre la API.

## Robustecimiento técnico (resiliencia e interoperabilidad con el backend)

- **Reintentos con backoff exponencial** (`ManejadorReintentos`) ante fallas
  transitorias de red — solo en peticiones GET (idempotentes), hasta 3 intentos.
  Implementado a mano en vez de con Polly/`Microsoft.Extensions.Http.Resilience`
  porque este entorno no pudo verificar versiones de paquete contra NuGet;
  migrar a ese paquete es el siguiente paso natural si confirmas versiones válidas.
- **Sesión expirada, con renovación real** (`ManejadorSesionExpirada`): un 401
  fuera de `/api/autenticacion/*` intenta renovar la sesión con
  `POST /api/autenticacion/renovar-token` antes de rendirse. En una petición GET,
  reintenta automáticamente con el token nuevo (el usuario no nota nada). En
  POST/DELETE no reintenta automáticamente (no se puede clonar con garantías un
  cuerpo ya enviado una vez), pero el token queda renovado para la siguiente
  acción. Si la renovación falla (token de renovación vencido/revocado), recién
  ahí se cierra la sesión y Blazor redirige solo a `/login`.
- **Cerrar sesión revoca en el servidor**: `ServicioAutenticacion.CerrarSesionAsync`
  llama a `POST /api/autenticacion/cerrar-sesion` (best-effort) antes de limpiar
  el almacenamiento local — antes "cerrar sesión" solo borraba el token en el
  cliente, dejándolo utilizable por cualquiera que tuviera una copia.
- **Errores reales, no silenciosos** (`ExcepcionApi` + `ServicioApi.LanzarSiFallaAsync`):
  toda operación que falla parsea el `application/problem+json` (RFC 7807) o el
  `ValidationProblemDetails` que devuelve el backend, y cada página lo muestra
  en pantalla — antes las páginas devolvían `null` en silencio ante un error.
- **Paginación real** (`PaginaDto<T>`, espejo del backend): Documentos y Actas
  cargan de a 20 elementos con "Cargar más", en vez de traer la tabla completa.
- **Timeout explícito** de 20s por petición HTTP, para no dejar la UI esperando
  indefinidamente si el backend no responde.

## Alcance actual / próximos pasos

Esta primera versión cubre 1:1 los endpoints ya expuestos por el backend:
autenticación, empresas y sedes, documentos (registro, firma, renovación,
eliminación), actas, y administración de control de acceso (usuarios,
roles, perfiles, permisos, grupos). Cosas pendientes, atadas a que el
backend las exponga:

- Catálogo de tipos de documento (hoy se ingresa el ID manualmente al
  registrar un documento — no existe todavía un endpoint de consulta).
- Edición/eliminación de empresas, sedes, roles, perfiles y permisos (la API
  actual solo permite consultarlos o crear empresas/sedes).
- Sincronización de reuniones (`POST /api/sincronizacion-reuniones/{proveedor}`)
  y webhooks — pensados para integraciones servidor-a-servidor, no para UI.
- El Panel calcula "pendientes"/"vencidos" solo sobre los últimos 5 documentos
  cargados (etiquetado así en la UI) — al paginar en el backend, ya no se puede
  calcular ese conteo sobre la tabla completa sin traerla entera. Lo correcto
  es que el backend exponga un endpoint de resumen/agregado (ej.
  `GET /api/documentos/resumen`) que devuelva esos conteos ya calculados en la
  base de datos.

## Notas de compilación

Este código se generó y revisó manualmente línea por línea, pero no pudo
compilarse en el entorno donde se creó (sin acceso a NuGet). Antes de dar
por buena la primera compilación local, corre:

```bash
dotnet restore
dotnet build
```

y reporta cualquier error de compilación para corregirlo.
