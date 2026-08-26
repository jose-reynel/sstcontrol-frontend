namespace SstControl.Frontend.Shared.Models;

/// <summary>Credenciales enviadas a POST /api/autenticacion/iniciar-sesion.</summary>
public record PeticionInicioSesion(string NombreUsuario, string Clave);

/// <summary>Respuesta del backend con el token JWT y los roles/permisos efectivos.</summary>
public record ResultadoAutenticacionDto(string Token, string NombreCompleto, List<string> Roles, List<string> Permisos);
