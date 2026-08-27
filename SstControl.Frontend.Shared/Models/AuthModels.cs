namespace SstControl.Frontend.Shared.Models;

/// <summary>Credenciales enviadas a POST /api/autenticacion/iniciar-sesion.</summary>
public record PeticionInicioSesion(string NombreUsuario, string Clave);

/// <summary>Token de renovación enviado a /renovar-token o /cerrar-sesion.</summary>
public record PeticionTokenRenovacion(string TokenRenovacion);

/// <summary>Respuesta del backend con el JWT, el token de renovación (larga
/// duración, opaco) y los roles/permisos efectivos del usuario.</summary>
public record ResultadoAutenticacionDto(string Token, string TokenRenovacion, string NombreCompleto, List<string> Roles, List<string> Permisos);
