using System.Net.Http.Json;
using System.Text.Json;
using SstControl.Frontend.Shared.Models;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Cliente tipado de la API de SstControl. Cada método corresponde a un endpoint
/// del backend (ver SstControl.Api/Controllers) — nombres y rutas alineados 1:1
/// con SstControl.Api.Controladores para que sea fácil ubicar el par
/// petición/endpoint al mantener ambos repos.
///
/// Toda operación que puede fallar por una razón que el usuario deba conocer
/// (validación, permisos, conflicto) lanza <see cref="ExcepcionApi"/> con el
/// mensaje ya listo para mostrar — nunca devuelve null en silencio.
/// </summary>
public class ServicioApi(HttpClient http)
{
    // ---- Empresas y sedes ----
    public async Task<List<EmpresaDto>> ObtenerEmpresasAsync() =>
        await http.GetFromJsonAsync<List<EmpresaDto>>("api/empresas") ?? [];

    public async Task<EmpresaDto> CrearEmpresaAsync(string nombre)
    {
        var respuesta = await http.PostAsJsonAsync("api/empresas", nombre);
        await LanzarSiFallaAsync(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<EmpresaDto>())!;
    }

    public async Task<SedeDto> CrearSedeAsync(int idEmpresa, string nombre)
    {
        var respuesta = await http.PostAsJsonAsync($"api/empresas/{idEmpresa}/sedes", nombre);
        await LanzarSiFallaAsync(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<SedeDto>())!;
    }

    // ---- Documentos ----
    /// <summary>Página de documentos, la más reciente primero. Ver PaginaDto para
    /// el total real (TotalElementos) — no asumas que Elementos.Count es el total.</summary>
    public async Task<PaginaDto<DocumentoDto>> ObtenerDocumentosAsync(int pagina = 1, int tamanioPagina = 20) =>
        await http.GetFromJsonAsync<PaginaDto<DocumentoDto>>($"api/documentos?pagina={pagina}&tamanioPagina={tamanioPagina}")
        ?? new PaginaDto<DocumentoDto>([], pagina, tamanioPagina, 0);

    public async Task<DocumentoDto> CrearDocumentoAsync(CrearDocumentoDto datos)
    {
        var respuesta = await http.PostAsJsonAsync("api/documentos", datos);
        await LanzarSiFallaAsync(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<DocumentoDto>())!;
    }

    public async Task<DocumentoDto> FirmarDocumentoAsync(int idDocumento)
    {
        var respuesta = await http.PostAsync($"api/documentos/{idDocumento}/firmar", content: null);
        await LanzarSiFallaAsync(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<DocumentoDto>())!;
    }

    public async Task<DocumentoDto> RenovarDocumentoAsync(int idDocumento)
    {
        var respuesta = await http.PostAsync($"api/documentos/{idDocumento}/renovar", content: null);
        await LanzarSiFallaAsync(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<DocumentoDto>())!;
    }

    public async Task EliminarDocumentoAsync(int idDocumento) =>
        await LanzarSiFallaAsync(await http.DeleteAsync($"api/documentos/{idDocumento}"));

    // ---- Actas ----
    public async Task<PaginaDto<ActaDto>> ObtenerActasAsync(int pagina = 1, int tamanioPagina = 20) =>
        await http.GetFromJsonAsync<PaginaDto<ActaDto>>($"api/actas?pagina={pagina}&tamanioPagina={tamanioPagina}")
        ?? new PaginaDto<ActaDto>([], pagina, tamanioPagina, 0);

    public async Task<ActaDto> CrearActaAsync(CrearActaDto datos)
    {
        var respuesta = await http.PostAsJsonAsync("api/actas", datos);
        await LanzarSiFallaAsync(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<ActaDto>())!;
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

    public async Task AsignarRolAsync(int idUsuario, int idRol) =>
        await LanzarSiFallaAsync(await http.PostAsJsonAsync("api/control-acceso/asignar-rol", new AsignarRolDto(idUsuario, idRol)));

    public async Task AsignarGrupoAsync(int idUsuario, int idGrupo) =>
        await LanzarSiFallaAsync(await http.PostAsJsonAsync("api/control-acceso/asignar-grupo", new AsignarGrupoDto(idUsuario, idGrupo)));

    /// <summary>Traduce una respuesta HTTP fallida en una ExcepcionApi con mensaje
    /// legible, leyendo el application/problem+json (RFC 7807) que devuelve
    /// SstControl.Api.Middleware.ManejadorErroresGlobal, o el ValidationProblemDetails
    /// automático de [ApiController] cuando falla una validación de DataAnnotations.</summary>
    private static async Task LanzarSiFallaAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode) return;

        string mensaje = respuesta.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Tu sesión expiró. Vuelve a iniciar sesión.",
            System.Net.HttpStatusCode.Forbidden => "No tienes permiso para realizar esta acción.",
            System.Net.HttpStatusCode.NotFound => "El recurso solicitado no existe.",
            System.Net.HttpStatusCode.TooManyRequests => "Demasiadas solicitudes. Intenta de nuevo en un momento.",
            _ => "Ocurrió un error al comunicarse con el servidor.",
        };

        try
        {
            var texto = await respuesta.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(texto))
            {
                using var documento = JsonDocument.Parse(texto);
                var raiz = documento.RootElement;

                // ValidationProblemDetails trae "errors": { "Campo": ["mensaje"] }.
                if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
                {
                    var primerError = errores.EnumerateObject().FirstOrDefault().Value;
                    if (primerError.ValueKind == JsonValueKind.Array && primerError.GetArrayLength() > 0)
                        mensaje = primerError[0].GetString() ?? mensaje;
                }
                // ProblemDetails "normal" trae "detail" (solo en Development) o "title".
                else if (raiz.TryGetProperty("detail", out var detalle) && detalle.ValueKind == JsonValueKind.String)
                    mensaje = detalle.GetString() ?? mensaje;
                else if (raiz.TryGetProperty("title", out var titulo) && titulo.ValueKind == JsonValueKind.String)
                    mensaje = titulo.GetString() ?? mensaje;
            }
        }
        catch (JsonException)
        {
            // El cuerpo no era JSON (ej. un 502 de un proxy intermedio) — se
            // conserva el mensaje genérico ya elegido según el código de estado.
        }

        throw new ExcepcionApi(mensaje, (int)respuesta.StatusCode);
    }
}
