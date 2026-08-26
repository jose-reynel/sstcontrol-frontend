using Microsoft.Extensions.Logging;
using SstControl.Frontend.Maui.Services;
using SstControl.Frontend.Shared.Services;

namespace SstControl.Frontend.Maui;

public static class MauiProgram
{
    /// <summary>URL base de la API para las builds de dispositivo/emulador.
    /// En Android, "localhost" dentro del emulador apunta al propio emulador, no
    /// a la PC anfitriona — por eso se usa el alias especial "10.0.2.2" que
    /// Android redirige al "localhost" de la máquina que lo aloja.</summary>
    private const string UrlBaseApiAndroid = "https://10.0.2.2:5080/";
    private const string UrlBaseApiOtros = "https://localhost:5080/";

    public static MauiApp CreateMauiApp()
    {
        // La interfaz completa vive dentro del BlazorWebView (HTML/CSS con las
        // fuentes web de SstControl.Frontend.Shared/wwwroot/css/app.css), por
        // lo que esta app no necesita registrar fuentes nativas de MAUI.
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // El token se guarda en el almacenamiento seguro del dispositivo.
        builder.Services.AddSingleton<ITokenStore, AlmacenTokenSeguro>();

        var urlBaseApi = OperatingSystem.IsAndroid() ? UrlBaseApiAndroid : UrlBaseApiOtros;
        builder.Services.AgregarServiciosSstControl(urlBaseApi);

        return builder.Build();
    }
}
