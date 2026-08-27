using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SstControl.Frontend.Shared.Models;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Ante un 401 (JWT expirado) fuera de /api/autenticacion/*, intenta renovar la
/// sesión con el token de renovación guardado (POST /api/autenticacion/renovar-token)
/// antes de rendirse. Si la renovación funciona:
///   - en una petición GET, reintenta automáticamente la misma petición con el
///     token nuevo — el usuario nunca nota que su sesión había expirado.
///   - en POST/DELETE, NO reintenta automáticamente: el contenido de la petición
///     original ya pudo haberse transmitido una vez y clonarlo seguro no está
///     garantizado para todo tipo de contenido (mismo motivo por el que
///     ManejadorReintentos tampoco reintenta escrituras). Sí deja el token ya
///     renovado guardado, así que la siguiente acción del usuario funciona sin
///     fricción — en el peor caso, esa acción puntual pide reintentar a mano.
/// Si la renovación falla (token de renovación vencido/revocado/inexistente), se
/// cierra la sesión local: al quedar el usuario "anónimo", CascadingAuthenticationState
/// redirige solo a /login (ver RedirectToLogin.razor), sin necesitar un
/// NavigationManager aquí.
/// </summary>
public class ManejadorSesionExpirada(ITokenStore almacenToken, ProveedorEstadoAutenticacion proveedorEstado) : DelegatingHandler
{
    // Evita que varias peticiones en paralelo, todas con 401 a la vez, disparen
    // renovaciones simultáneas — el backend rota el token en cada uso, así que la
    // segunda renovación en paralelo usaría un token ya revocado por la primera.
    private static readonly SemaphoreSlim BloqueoRenovacion = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
    {
        var esRutaDeAutenticacion = peticion.RequestUri?.AbsolutePath.Contains("/api/autenticacion/", StringComparison.OrdinalIgnoreCase) ?? false;
        var respuesta = await base.SendAsync(peticion, cancelacion);

        if (respuesta.StatusCode != HttpStatusCode.Unauthorized || esRutaDeAutenticacion)
            return respuesta;

        var tokenNuevo = await IntentarRenovarAsync(peticion.RequestUri!, cancelacion);
        if (tokenNuevo is null)
        {
            await almacenToken.LimpiarTokenAsync();
            proveedorEstado.NotificarCambio();
            return respuesta;
        }

        if (peticion.Method != HttpMethod.Get)
            return respuesta; // token ya renovado para la próxima acción; esta se reporta como fallida.

        respuesta.Dispose();
        var reintento = ClonarConToken(peticion, tokenNuevo);
        return await base.SendAsync(reintento, cancelacion);
    }

    /// <summary>Devuelve el JWT nuevo si logró renovar, o null si el token de
    /// renovación ya no sirve (hay que iniciar sesión de nuevo).</summary>
    private async Task<string?> IntentarRenovarAsync(Uri uriOriginal, CancellationToken cancelacion)
    {
        await BloqueoRenovacion.WaitAsync(cancelacion);
        try
        {
            // Otra petición concurrente pudo haber renovado mientras esperábamos el turno.
            var tokenRenovacion = await almacenToken.ObtenerTokenRenovacionAsync();
            if (string.IsNullOrWhiteSpace(tokenRenovacion)) return null;

            var uriRenovacion = new Uri(uriOriginal, "/api/autenticacion/renovar-token");
            using var peticionRenovacion = new HttpRequestMessage(HttpMethod.Post, uriRenovacion)
            {
                Content = JsonContent.Create(new PeticionTokenRenovacion(tokenRenovacion)),
            };

            using var respuestaRenovacion = await base.SendAsync(peticionRenovacion, cancelacion);
            if (!respuestaRenovacion.IsSuccessStatusCode) return null;

            var resultado = await respuestaRenovacion.Content.ReadFromJsonAsync<ResultadoAutenticacionDto>(cancellationToken: cancelacion);
            if (resultado is null) return null;

            await almacenToken.GuardarTokensAsync(resultado.Token, resultado.TokenRenovacion);
            proveedorEstado.NotificarCambio();
            return resultado.Token;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        finally
        {
            BloqueoRenovacion.Release();
        }
    }

    private static HttpRequestMessage ClonarConToken(HttpRequestMessage original, string token)
    {
        var clon = new HttpRequestMessage(original.Method, original.RequestUri);
        foreach (var encabezado in original.Headers)
        {
            if (encabezado.Key == "Authorization") continue;
            clon.Headers.TryAddWithoutValidation(encabezado.Key, encabezado.Value);
        }
        clon.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return clon;
    }
}
