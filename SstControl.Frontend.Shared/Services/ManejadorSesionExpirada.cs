using System.Net;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Si la API responde 401 mientras había una sesión activa, el token dejó de
/// ser válido (expiró, o fue revocado) — este handler la cierra del lado del
/// cliente y notifica al proveedor de autenticación. No navega explícitamente:
/// al quedar el usuario "anónimo", CascadingAuthenticationState re-evalúa
/// automáticamente el árbol y AuthorizeRouteView redirige solo a /login (ver
/// RedirectToLogin.razor) — el mismo mecanismo que protege cualquier ruta con
/// [Authorize], sin necesitar un NavigationManager aquí.
///
/// Se ignora un 401 en /api/autenticacion/iniciar-sesion: ahí un 401 es
/// simplemente "credenciales incorrectas", no una sesión que expiró, y no hay
/// token que limpiar.
/// </summary>
public class ManejadorSesionExpirada(ITokenStore almacenToken, ProveedorEstadoAutenticacion proveedorEstado) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
    {
        var respuesta = await base.SendAsync(peticion, cancelacion);

        var esPeticionDeLogin = peticion.RequestUri?.AbsolutePath.EndsWith("iniciar-sesion", StringComparison.OrdinalIgnoreCase) ?? false;

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized && !esPeticionDeLogin)
        {
            var habiaToken = !string.IsNullOrWhiteSpace(await almacenToken.ObtenerTokenAsync());
            if (habiaToken)
            {
                await almacenToken.LimpiarTokenAsync();
                proveedorEstado.NotificarCambio();
            }
        }

        return respuesta;
    }
}
