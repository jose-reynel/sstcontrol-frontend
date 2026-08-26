namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Abstracción sobre dónde vive el token JWT entre sesiones. Cada proyecto host
/// aporta su propia implementación: la app Web lo guarda en localStorage del
/// navegador (ver SstControl.Frontend.Web) y la app Maui lo guarda en el
/// almacenamiento seguro del dispositivo (ver SstControl.Frontend.Maui).
/// </summary>
public interface ITokenStore
{
    Task<string?> ObtenerTokenAsync();
    Task GuardarTokenAsync(string token);
    Task LimpiarTokenAsync();
}
