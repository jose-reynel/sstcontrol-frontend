using System.Net.Http.Json;
using SstControl.Frontend.Shared.Models;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Cliente tipado de la API de SstControl. Cada método corresponde a un endpoint
/// del backend (ver SstControl.Api/Controllers) — nombres y rutas alineados 1:1
/// con SstControl.Api.Controladores para que sea fácil ubicar el par
/// petición/endpoint al mantener ambos repos.
/// </summary>
public class ServicioApi(HttpClient http)
{
    // ---- Empresas y sedes ----
    public async Task<List<EmpresaDto>> ObtenerEmpresasAsync() =>
        await http.GetFromJsonAsync<List<EmpresaDto>>("api/empresas") ?? [];

    public async Task<EmpresaDto?> CrearEmpresaAsync(string nombre)
    {
        var respuesta = await http.PostAsJsonAsync("api/empresas", nombre);
        return respuesta.IsSuccessStatusCode ? await respuesta.Content.ReadFromJsonAsync<EmpresaDto>() : null;
    }

    public async Task<SedeDto?> CrearSedeAsync(int idEmpresa, string nombre)
    {
        var respuesta = await http.PostAsJsonAsync($"api/empresas/{idEmpresa}/sedes", nombre);
        return respuesta.IsSuccessStatusCode ? await respuesta.Content.ReadFromJsonAsync<SedeDto>() : null;
    }

    // ---- Documentos ----
    public async Task<List<DocumentoDto>> ObtenerDocumentosAsync() =>
        await http.GetFromJsonAsync<List<DocumentoDto>>("api/documentos") ?? [];

    public async Task<DocumentoDto?> CrearDocumentoAsync(CrearDocumentoDto datos)
    {
        var respuesta = await http.PostAsJsonAsync("api/documentos", datos);
        return respuesta.IsSuccessStatusCode ? await respuesta.Content.ReadFromJsonAsync<DocumentoDto>() : null;
    }

    public async Task<DocumentoDto?> FirmarDocumentoAsync(int idDocumento)
    {
        var respuesta = await http.PostAsync($"api/documentos/{idDocumento}/firmar", content: null);
        return respuesta.IsSuccessStatusCode ? await respuesta.Content.ReadFromJsonAsync<DocumentoDto>() : null;
    }

    public async Task<DocumentoDto?> RenovarDocumentoAsync(int idDocumento)
    {
        var respuesta = await http.PostAsync($"api/documentos/{idDocumento}/renovar", content: null);
        return respuesta.IsSuccessStatusCode ? await respuesta.Content.ReadFromJsonAsync<DocumentoDto>() : null;
    }

    public async Task<bool> EliminarDocumentoAsync(int idDocumento) =>
        (await http.DeleteAsync($"api/documentos/{idDocumento}")).IsSuccessStatusCode;

    // ---- Actas ----
    public async Task<List<ActaDto>> ObtenerActasAsync() =>
        await http.GetFromJsonAsync<List<ActaDto>>("api/actas") ?? [];

    public async Task<ActaDto?> CrearActaAsync(CrearActaDto datos)
    {
        var respuesta = await http.PostAsJsonAsync("api/actas", datos);
        return respuesta.IsSuccessStatusCode ? await respuesta.Content.ReadFromJsonAsync<ActaDto>() : null;
    }

    // ---- Control de acceso (RBAC) ----
    public async Task<List<PermisoDto>> ObtenerPermisosAsync() =>
        await http.GetFromJsonAsync<List<PermisoDto>>("api/control-acceso/permisos") ?? [];

    public async Task<List<PerfilDto>> ObtenerPerfilesAsync() =>
        await http.GetFromJsonAsync<List<PerfilDto>>("api/control-acceso/perfiles") ?? [];

    public async Task<List<RolDto>> ObtenerRolesAsync() =>
        await http.GetFromJsonAsync<List<RolDto>>("api/control-acceso/roles") ?? [];

    public async Task<List<GrupoDto>> ObtenerGruposAsync() =>
        await http.GetFromJsonAsync<List<GrupoDto>>("api/control-acceso/grupos") ?? [];

    public async Task<List<UsuarioResumenDto>> ObtenerUsuariosAsync() =>
        await http.GetFromJsonAsync<List<UsuarioResumenDto>>("api/control-acceso/usuarios") ?? [];

    public async Task<bool> AsignarRolAsync(int idUsuario, int idRol) =>
        (await http.PostAsJsonAsync("api/control-acceso/asignar-rol", new AsignarRolDto(idUsuario, idRol))).IsSuccessStatusCode;

    public async Task<bool> AsignarGrupoAsync(int idUsuario, int idGrupo) =>
        (await http.PostAsJsonAsync("api/control-acceso/asignar-grupo", new AsignarGrupoDto(idUsuario, idGrupo))).IsSuccessStatusCode;
}
