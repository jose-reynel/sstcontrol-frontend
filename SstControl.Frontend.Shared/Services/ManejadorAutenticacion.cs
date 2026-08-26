using System.Net.Http.Headers;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// DelegatingHandler registrado en el HttpClient nombrado "SstControlApi": agrega
/// automáticamente el header "Authorization: Bearer {token}" a cada petición,
/// tomando el token del ITokenStore vigente en la plataforma (Web o Maui).
/// </summary>
public class ManejadorAutenticacion(ITokenStore almacenToken) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
    {
        var token = await almacenToken.ObtenerTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(peticion, cancelacion);
    }
}
