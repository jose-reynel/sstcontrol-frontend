using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Fuente de verdad de "quién está autenticado" para Blazor (AuthorizeView,
/// [Authorize], etc.). Al arrancar, y tras iniciar/cerrar sesión, reconstruye el
/// ClaimsPrincipal a partir del token guardado en el ITokenStore de la plataforma.
/// </summary>
public class ProveedorEstadoAutenticacion(ITokenStore almacenToken) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonimo = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await almacenToken.ObtenerTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return new AuthenticationState(Anonimo);

        var identidad = DecodificadorJwt.CrearIdentidad(token, "jwt");
        if (DecodificadorJwt.EstaExpirado(identidad))
        {
            await almacenToken.LimpiarTokenAsync();
            return new AuthenticationState(Anonimo);
        }

        return new AuthenticationState(new ClaimsPrincipal(identidad));
    }

    /// <summary>Fuerza a Blazor a releer el estado de autenticación (tras login/logout).</summary>
    public void NotificarCambio() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
