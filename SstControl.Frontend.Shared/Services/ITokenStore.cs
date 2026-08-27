namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Abstracción sobre dónde viven el JWT y el token de renovación entre sesiones.
/// Cada proyecto host aporta su propia implementación: la app Web los guarda en
/// localStorage del navegador (ver SstControl.Frontend.Web) y la app Maui los
/// guarda en el almacenamiento seguro del dispositivo (ver SstControl.Frontend.Maui).
/// </summary>
public interface ITokenStore
{
    Task<string?> ObtenerTokenAsync();
    Task<string?> ObtenerTokenRenovacionAsync();

    /// <summary>Guarda ambos tokens de una vez — siempre se emiten juntos (login o
    /// renovación), así que no tiene sentido poder guardar uno sin el otro.</summary>
    Task GuardarTokensAsync(string token, string tokenRenovacion);

    Task LimpiarTokenAsync();
}
