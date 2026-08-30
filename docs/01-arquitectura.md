# Manual técnico (frontend) — Arquitectura

## Proyectos de la solución

```
SstControl.Frontend.slnx
├── SstControl.Frontend.Shared   Toda la UI (Razor Class Library) — pages, layout,
│                                servicios, modelos. Web y Maui SOLO renderizan
│                                <Routes /> de este proyecto; no tienen sus propias
│                                páginas.
├── SstControl.Frontend.Web      Host Blazor WebAssembly (PWA, instalable, offline
│                                parcial vía Service Worker)
└── SstControl.Frontend.Maui     Host .NET MAUI Blazor Hybrid — Android, iOS,
                                 macOS (Catalyst) y Windows desde el mismo código
```

Principio de diseño: **cada página, cada llamada a la API y cada regla de
permisos se escribe una sola vez**, en `SstControl.Frontend.Shared`. Los dos
proyectos "host" solo aportan lo específico de la plataforma: dónde se
guarda el token de sesión (`ITokenStore`) y la página/ventana donde se monta
Blazor.

## Páginas (todas en `SstControl.Frontend.Shared/Pages`)

| Página | Ruta | Permiso requerido |
|---|---|---|
| `Login.razor` | `/login` | — (pública) |
| `Dashboard.razor` | `/` | cualquier autenticado |
| `Documentos.razor` | `/documentos` | cualquier autenticado (acciones específicas gated por permiso, ver abajo) |
| `Empresas.razor` | `/empresas` | cualquier autenticado (crear gated por `empresas.gestionar`) |
| `Actas.razor` | `/actas` | `actas.ver` (crear gated por `actas.crear`) |
| `ActaCompromisos.razor` | `/actas/{id}/compromisos` | `actas.ver` (acciones gated por `actas.crear`) |
| `Administracion/Usuarios.razor` | `/administracion/usuarios` | `accesos.administrar` |
| `Administracion/Roles.razor` | `/administracion/roles` | `accesos.administrar` |
| `Administracion/Perfiles.razor` | `/administracion/perfiles` | `accesos.administrar` |
| `Administracion/Permisos.razor` | `/administracion/permisos` | `accesos.administrar` |
| `Administracion/Grupos.razor` | `/administracion/grupos` | `accesos.administrar` |

La UI **no reemplaza** la autorización real (que siempre la valida la API) —
solo evita mostrar botones/pantallas que la API igual rechazaría, para no
confundir al usuario con acciones que no puede completar.

## Servicios (`SstControl.Frontend.Shared/Services`)

| Servicio | Responsabilidad |
|---|---|
| `ServicioApi` | Cliente tipado de la API — un método por endpoint. Traduce cualquier error del backend a `ExcepcionApi` con mensaje legible. |
| `ServicioAutenticacion` | Login/logout; guarda ambos tokens (JWT + renovación) vía `ITokenStore`. |
| `ITokenStore` | Abstracción sobre dónde vive el token — implementada distinto en cada host (ver abajo). |
| `ManejadorAutenticacion` | `DelegatingHandler` que agrega `Authorization: Bearer` a cada petición. |
| `ManejadorSesionExpirada` | Ante un 401, intenta renovar la sesión (`POST /api/autenticacion/renovar-token`) antes de cerrarla. |
| `ManejadorReintentos` | Reintentos con backoff exponencial en peticiones GET ante fallas transitorias de red. |
| `ProveedorEstadoAutenticacion` | Reconstruye el `ClaimsPrincipal` de Blazor decodificando el JWT guardado. |
| `DecodificadorJwt` / `ExtensionesClaimsPrincipal` | Utilidades para leer roles/permisos del JWT sin otra llamada a la API. |

Orden real de la cadena HTTP (`ConfiguracionServicios.AgregarServiciosSstControl`):
`ManejadorAutenticacion` → `ManejadorSesionExpirada` → `ManejadorReintentos`
→ red. Ver el frontend `README.md` para el detalle de por qué ese orden
importa.

## Cada plataforma aporta su propio `ITokenStore`
- **Web** (`SstControl.Frontend.Web/Services/AlmacenTokenNavegador.cs`):
  `localStorage` del navegador, vía interop con JS.
- **Maui** (`SstControl.Frontend.Maui/Services/AlmacenTokenSeguro.cs`):
  `Microsoft.Maui.Storage.SecureStorage` — Keychain en iOS/Mac, KeyStore en
  Android, DPAPI en Windows.

## Sistema de diseño
`SstControl.Frontend.Shared/wwwroot/css/app.css` — paleta, tipografías
(Space Grotesk / Inter / IBM Plex Mono) y componentes (insignias de estado,
tarjetas, el "sello" de firma/aprobación). Compartido entre Web y Maui vía
el mecanismo de *static web assets* de las Razor Class Library
(`_content/SstControl.Frontend.Shared/css/app.css`).
