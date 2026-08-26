using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SstControl.Frontend.Shared;
using SstControl.Frontend.Shared.Services;
using SstControl.Frontend.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// El componente raíz vive en la librería compartida (Routes.razor), así que
// tanto la app Web como la app Maui arrancan exactamente el mismo árbol de UI.
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// El token se guarda en localStorage del navegador (ver AlmacenTokenNavegador).
builder.Services.AddScoped<ITokenStore, AlmacenTokenNavegador>();

var urlBaseApi = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AgregarServiciosSstControl(urlBaseApi);

await builder.Build().RunAsync();
