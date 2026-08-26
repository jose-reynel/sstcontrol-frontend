using System.Net.Http.Json;
using SstControl.Frontend.Shared.Models;

namespace SstControl.Frontend.Shared.Services;

/// <summary>Inicio y cierre de sesión contra la API, y sincronización del estado de autenticación de Blazor.</summary>
public class ServicioAutenticacion(HttpClient http, ITokenStore almacenToken, ProveedorEstadoAutenticacion proveedorEstado)
{
    public async Task<(bool Exito, string? Error)> IniciarSesionAsync(string nombreUsuario, string clave)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await http.PostAsJsonAsync("api/autenticacion/iniciar-sesion", new PeticionInicioSesion(nombreUsuario, clave));
        }
        catch (HttpRequestException)
        {
            return (false, "No se pudo contactar al servidor. Verifica tu conexión.");
        }

        if (!respuesta.IsSuccessStatusCode)
            return (false, "Usuario o contraseña incorrectos.");

        var resultado = await respuesta.Content.ReadFromJsonAsync<ResultadoAutenticacionDto>();
        if (resultado is null) return (false, "El servidor respondió con datos inesperados.");

        await almacenToken.GuardarTokenAsync(resultado.Token);
        proveedorEstado.NotificarCambio();
        return (true, null);
    }

    public async Task CerrarSesionAsync()
    {
        await almacenToken.LimpiarTokenAsync();
        proveedorEstado.NotificarCambio();
    }
}
