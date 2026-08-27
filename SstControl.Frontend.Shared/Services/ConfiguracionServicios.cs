using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Registro único de todos los servicios de la app (HttpClient con auth, servicios
/// de dominio, proveedor de autenticación). Tanto SstControl.Frontend.Web como
/// SstControl.Frontend.Maui llaman a esta misma extensión, pasando su propia URL
/// base de la API — así ambas plataformas quedan siempre en sincronía.
/// Requiere que el host ya haya registrado un ITokenStore antes de llamarla.
/// </summary>
public static class ConfiguracionServicios
{
    public static IServiceCollection AgregarServiciosSstControl(this IServiceCollection servicios, string urlBaseApi)
    {
        servicios.AddScoped<AuthenticationStateProvider, ProveedorEstadoAutenticacion>();
        servicios.AddScoped(sp => (ProveedorEstadoAutenticacion)sp.GetRequiredService<AuthenticationStateProvider>());

        servicios.AddTransient<ManejadorAutenticacion>();
        servicios.AddTransient<ManejadorSesionExpirada>();
        servicios.AddTransient<ManejadorReintentos>();

        // Orden de la cadena (de afuera hacia adentro): agrega el token → detecta
        // sesión expirada sobre la respuesta final → reintenta fallas transitorias
        // (los reintentos ya salen con el token puesto, gracias al orden).
        servicios.AddHttpClient("SstControlApi", cliente =>
            {
                cliente.BaseAddress = new Uri(urlBaseApi);
                cliente.Timeout = TimeSpan.FromSeconds(20);
            })
            .AddHttpMessageHandler<ManejadorAutenticacion>()
            .AddHttpMessageHandler<ManejadorSesionExpirada>()
            .AddHttpMessageHandler<ManejadorReintentos>();

        servicios.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SstControlApi"));

        servicios.AddScoped<ServicioAutenticacion>();
        servicios.AddScoped<ServicioApi>();
        servicios.AddAuthorizationCore();

        return servicios;
    }
}
