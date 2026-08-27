namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Excepción lanzada por ServicioApi cuando la API responde con un error. Trae
/// el mensaje ya legible para mostrar directamente en la UI (parseado del
/// application/problem+json que devuelve SstControl.Api.Middleware.ManejadorErroresGlobal
/// o de la respuesta de validación automática de [ApiController]), en vez de
/// que cada página tenga que adivinar por qué falló una petición.
/// </summary>
public class ExcepcionApi(string mensaje, int? codigoEstado) : Exception(mensaje)
{
    public int? CodigoEstado { get; } = codigoEstado;
}
