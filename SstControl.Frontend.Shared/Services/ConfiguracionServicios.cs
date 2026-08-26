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
        servicios.AddTransient<ManejadorAutenticacion>();

        servicios.AddHttpClient("SstControlApi", cliente => cliente.BaseAddress = new Uri(urlBaseApi))
            .AddHttpMessageHandler<ManejadorAutenticacion>();

        servicios.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SstControlApi"));

        servicios.AddScoped<AuthenticationStateProvider, ProveedorEstadoAutenticacion>();
        servicios.AddScoped(sp => (ProveedorEstadoAutenticacion)sp.GetRequiredService<AuthenticationStateProvider>());
        servicios.AddScoped<ServicioAutenticacion>();
        servicios.AddScoped<ServicioApi>();
        servicios.AddAuthorizationCore();

        return servicios;
    }
}
