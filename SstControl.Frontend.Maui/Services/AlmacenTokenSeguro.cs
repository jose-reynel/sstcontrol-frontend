using SstControl.Frontend.Shared.Services;

namespace SstControl.Frontend.Maui.Services;

/// <summary>Implementación de ITokenStore para la app Maui: usa el almacenamiento
/// seguro cifrado del sistema operativo (Keychain en iOS/Mac, KeyStore en
/// Android, DPAPI en Windows) provisto por Microsoft.Maui.Storage.SecureStorage.</summary>
public class AlmacenTokenSeguro : ITokenStore
{
    private const string Clave = "sstcontrol_token";

    public Task<string?> ObtenerTokenAsync() => SecureStorage.Default.GetAsync(Clave);

    public Task GuardarTokenAsync(string token) => SecureStorage.Default.SetAsync(Clave, token);

    public Task LimpiarTokenAsync()
    {
        SecureStorage.Default.Remove(Clave);
        return Task.CompletedTask;
    }
}
