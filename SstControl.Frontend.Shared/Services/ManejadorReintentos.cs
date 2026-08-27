namespace SstControl.Frontend.Shared.Services;

/// <summary>
/// Reintentos con backoff exponencial ante fallas transitorias (Wi-Fi
/// inestable en el celular, un despliegue en curso del backend, un timeout
/// puntual). Solo reintenta peticiones GET — son idempotentes por definición;
/// reintentar un POST/DELETE automáticamente podría duplicar una acción
/// (crear el mismo documento dos veces, por ejemplo) si la primera petición sí
/// llegó al servidor pero la respuesta se perdió en el camino.
///
/// Implementado a mano (sin Polly/Microsoft.Extensions.Http.Resilience) porque
/// este entorno no tiene forma de verificar contra NuGet qué versión exacta de
/// esos paquetes es compatible con .NET 10 en este momento — una versión mal
/// elegida rompería la restauración. Si luego confirmas versiones válidas,
/// migrar a Microsoft.Extensions.Http.Resilience es el siguiente paso natural
/// (agrega jitter, circuit breaker y métricas out-of-the-box).
/// </summary>
public class ManejadorReintentos : DelegatingHandler
{
    private const int MaximoIntentos = 3;
    private static readonly TimeSpan EsperaBase = TimeSpan.FromMilliseconds(400);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
    {
        if (peticion.Method != HttpMethod.Get)
            return await base.SendAsync(peticion, cancelacion);

        for (var intento = 1; intento <= MaximoIntentos; intento++)
        {
            var esUltimoIntento = intento == MaximoIntentos;
            HttpResponseMessage respuesta;

            try
            {
                respuesta = await base.SendAsync(await ClonarPeticionAsync(peticion), cancelacion);
            }
            catch (HttpRequestException) when (!esUltimoIntento)
            {
                // Fallo de red (sin conexión, DNS, TLS): se reintenta igual que un 5xx.
                await Task.Delay(EsperaBase * Math.Pow(2, intento - 1), cancelacion);
                continue;
            }

            if (!EsFallaTransitoria(respuesta.StatusCode) || esUltimoIntento)
                return respuesta;

            respuesta.Dispose();
            await Task.Delay(EsperaBase * Math.Pow(2, intento - 1), cancelacion);
        }

        // Inalcanzable: el bucle siempre retorna o relanza en su última vuelta.
        // Solo existe para que el compilador vea un camino de retorno explícito.
        throw new InvalidOperationException("No se pudo completar la petición tras los reintentos.");
    }

    private static bool EsFallaTransitoria(System.Net.HttpStatusCode codigo) =>
        (int)codigo >= 500 || codigo == System.Net.HttpStatusCode.RequestTimeout || codigo == System.Net.HttpStatusCode.TooManyRequests;

    /// <summary>HttpRequestMessage no se puede reenviar dos veces — hay que clonarlo
    /// en cada intento (incluidos sus headers, ya que ManejadorAutenticacion corre
    /// antes en la cadena y ya agregó el Authorization en el mensaje original).</summary>
    private static async Task<HttpRequestMessage> ClonarPeticionAsync(HttpRequestMessage original)
    {
        var clon = new HttpRequestMessage(original.Method, original.RequestUri);
        foreach (var encabezado in original.Headers)
            clon.Headers.TryAddWithoutValidation(encabezado.Key, encabezado.Value);

        if (original.Content is not null)
        {
            var bytes = await original.Content.ReadAsByteArrayAsync();
            clon.Content = new ByteArrayContent(bytes);
            foreach (var encabezado in original.Content.Headers)
                clon.Content.Headers.TryAddWithoutValidation(encabezado.Key, encabezado.Value);
        }

        return clon;
    }
}
