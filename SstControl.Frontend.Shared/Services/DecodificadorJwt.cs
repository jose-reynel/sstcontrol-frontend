using System.Security.Claims;
using System.Text.Json;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Decodifica el payload de un JWT (sin validar la firma — eso ya lo hizo la API
/// al emitirlo) para reconstruir los claims del usuario en el cliente: nombre,
/// roles y permisos. Así el frontend no necesita volver a llamar a la API solo
/// para saber quién es el usuario tras recargar la página o reabrir la app.
/// </summary>
public static class DecodificadorJwt
{
    public static ClaimsIdentity CrearIdentidad(string token, string tipoAutenticacion)
    {
        var partes = token.Split('.');
        if (partes.Length != 3) return new ClaimsIdentity();

        var payload = partes[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

        byte[] bytes;
        try { bytes = Convert.FromBase64String(payload); }
        catch (FormatException) { return new ClaimsIdentity(); }

        using var documento = JsonDocument.Parse(bytes);
        var claims = new List<Claim>();
        foreach (var propiedad in documento.RootElement.EnumerateObject())
        {
            if (propiedad.Value.ValueKind == JsonValueKind.Array)
                claims.AddRange(propiedad.Value.EnumerateArray().Select(elemento => new Claim(propiedad.Name, elemento.ToString())));
            else
                claims.Add(new Claim(propiedad.Name, propiedad.Value.ToString()));
        }

        return new ClaimsIdentity(claims, tipoAutenticacion, ClaimTypes.Name, ClaimTypes.Role);
    }

    /// <summary>Indica si el claim "exp" (fecha de expiración, en segundos Unix) ya pasó.</summary>
    public static bool EstaExpirado(ClaimsIdentity identidad)
    {
        var claimExp = identidad.FindFirst("exp")?.Value;
        if (claimExp is null || !long.TryParse(claimExp, out var segundosUnix)) return false;
        return DateTimeOffset.FromUnixTimeSeconds(segundosUnix) <= DateTimeOffset.UtcNow;
    }
}
