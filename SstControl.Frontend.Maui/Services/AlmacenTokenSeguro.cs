using SstControl.Frontend.Shared.Services;

namespace SstControl.Frontend.Maui.Services;

/// <summary>Implementación de ITokenStore para la app Maui: usa el almacenamiento
/// seguro cifrado del sistema operativo (Keychain en iOS/Mac, KeyStore en
/// Android, DPAPI en Windows) provisto por Microsoft.Maui.Storage.SecureStorage.</summary>
public class AlmacenTokenSeguro : ITokenStore
{
    private const string ClaveToken = "sstcontrol_token";
    private const string ClaveTokenRenovacion = "sstcontrol_token_renovacion";

    public Task<string?> ObtenerTokenAsync() => SecureStorage.Default.GetAsync(ClaveToken);
    public Task<string?> ObtenerTokenRenovacionAsync() => SecureStorage.Default.GetAsync(ClaveTokenRenovacion);

    public async Task GuardarTokensAsync(string token, string tokenRenovacion)
    {
        await SecureStorage.Default.SetAsync(ClaveToken, token);
        await SecureStorage.Default.SetAsync(ClaveTokenRenovacion, tokenRenovacion);
    }

    public Task LimpiarTokenAsync()
    {
        SecureStorage.Default.Remove(ClaveToken);
        SecureStorage.Default.Remove(ClaveTokenRenovacion);
        return Task.CompletedTask;
    }
}
