using Microsoft.JSInterop;
using SstControl.Frontend.Shared.Services;

namespace SstControl.Frontend.Web.Services;

/// <summary>Implementación de ITokenStore para la app Web: guarda el JWT y el
/// token de renovación en el localStorage del navegador vía interop directo con
/// la API de JS del DOM.</summary>
public class AlmacenTokenNavegador(IJSRuntime js) : ITokenStore
{
    private const string ClaveToken = "sstcontrol_token";
    private const string ClaveTokenRenovacion = "sstcontrol_token_renovacion";

    public async Task<string?> ObtenerTokenAsync() => await ObtenerAsync(ClaveToken);
    public async Task<string?> ObtenerTokenRenovacionAsync() => await ObtenerAsync(ClaveTokenRenovacion);

    public async Task GuardarTokensAsync(string token, string tokenRenovacion)
    {
        await js.InvokeVoidAsync("localStorage.setItem", ClaveToken, token);
        await js.InvokeVoidAsync("localStorage.setItem", ClaveTokenRenovacion, tokenRenovacion);
    }

    public async Task LimpiarTokenAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", ClaveToken);
        await js.InvokeVoidAsync("localStorage.removeItem", ClaveTokenRenovacion);
    }

    private async Task<string?> ObtenerAsync(string clave)
    {
        try { return await js.InvokeAsync<string?>("localStorage.getItem", clave); }
        catch (JSException) { return null; }
    }
}
