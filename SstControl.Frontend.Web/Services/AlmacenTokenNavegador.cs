using Microsoft.JSInterop;
using SstControl.Frontend.Shared.Services;

namespace SstControl.Frontend.Web.Services;

/// <summary>Implementación de ITokenStore para la app Web: guarda el JWT en el
/// localStorage del navegador vía interop directo con la API de JS del DOM.</summary>
public class AlmacenTokenNavegador(IJSRuntime js) : ITokenStore
{
    private const string Clave = "sstcontrol_token";

    public async Task<string?> ObtenerTokenAsync()
    {
        try { return await js.InvokeAsync<string?>("localStorage.getItem", Clave); }
        catch (JSException) { return null; }
    }

    public async Task GuardarTokenAsync(string token) =>
        await js.InvokeVoidAsync("localStorage.setItem", Clave, token);

    public async Task LimpiarTokenAsync() =>
        await js.InvokeVoidAsync("localStorage.removeItem", Clave);
}
