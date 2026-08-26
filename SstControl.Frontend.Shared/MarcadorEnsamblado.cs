namespace SstControl.Frontend.Shared;

/// <summary>
/// Tipo marcador sin lógica: existe únicamente para que el Router (definido en
/// Routes.razor) pueda ubicar el ensamblado donde viven todas las páginas
/// (@page) del sistema, ya que todas están en esta librería compartida y no
/// en los proyectos host (Web / Maui).
/// </summary>
public sealed class MarcadorEnsamblado
{
    private MarcadorEnsamblado() { }
}
