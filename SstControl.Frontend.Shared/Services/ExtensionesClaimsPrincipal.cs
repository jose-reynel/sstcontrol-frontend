using System.Security.Claims;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Espejo en el cliente de SstControl.Api.Seguridad.ExtensionesPermisos: el JWT
/// trae cada permiso efectivo del usuario como un claim de tipo "permiso", y esta
/// extensión es el único punto donde se pregunta por él — así la UI puede
/// mostrar/ocultar acciones según el mismo RBAC que ya exige la API.
/// </summary>
public static class ExtensionesClaimsPrincipal
{
    public const string TipoClaimPermiso = "permiso";

    public static bool TienePermiso(this ClaimsPrincipal usuario, string codigoPermiso) =>
        usuario.Claims.Any(c => c.Type == TipoClaimPermiso && c.Value == codigoPermiso);
}
