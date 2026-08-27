namespace SstControl.Frontend.Shared.Models;

public record PermisoDto(int IdPermiso, string Codigo, string Descripcion, string Modulo);
public record PerfilDto(int IdPerfil, string Nombre, string? Descripcion, List<string> CodigosPermiso);
public record RolDto(int IdRol, string Nombre, string? Descripcion, List<string> NombresPerfiles);
public record GrupoDto(int IdGrupo, string Nombre, int? IdEmpresa, List<string> UsuariosDelGrupo);
public record UsuarioResumenDto(int IdUsuario, string NombreUsuario, string NombreCompleto, List<string> Roles, List<string> Grupos);

public record AsignarRolDto(int IdUsuario, int IdRol);
public record AsignarGrupoDto(int IdUsuario, int IdGrupo);

/// <summary>Envoltorio de paginación — espejo de PaginaDto&lt;T&gt; en el backend
/// (SstControl.Aplicacion.DTOs). Se define acá, no en un archivo propio, para
/// mantener el mismo criterio de "un archivo por área" que ya usan los demás
/// modelos compartidos.</summary>
public record PaginaDto<T>(List<T> Elementos, int Pagina, int TamanioPagina, int TotalElementos)
{
    public int TotalPaginas => TamanioPagina <= 0 ? 0 : (int)Math.Ceiling(TotalElementos / (double)TamanioPagina);
    public bool HayMas => Pagina < TotalPaginas;
}
