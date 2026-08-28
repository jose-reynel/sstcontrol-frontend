namespace SstControl.Frontend.Shared.Models;

/// <summary>Compromiso/acuerdo de seguimiento de una Acta — espejo de
/// CompromisoActaDto en el backend. Origen "Bot" = lo generó el bot de minutas
/// a partir del contenido de la reunión; "Manual" = lo agregó una persona.</summary>
public record CompromisoActaDto(int IdCompromiso, int IdActa, string Descripcion, string? Responsable,
    DateOnly? FechaLimite, string Estado, string Origen, int? IdDocumentoRelacionado, string? ActividadDocumentoRelacionado);

public record CrearCompromisoDto(string Descripcion, string? Responsable, DateOnly? FechaLimite, int? IdDocumentoRelacionado);

public record VincularDocumentoDto(int IdDocumento);

/// <summary>Resultado de correr el bot de minutas sobre el contenido ya
/// sincronizado de una reunión: el extracto de texto que usó como fuente
/// (null si la reunión no trajo transcripción/resumen) y todos los
/// compromisos del acta (los nuevos que detectó + los que ya existían).</summary>
public record MinutaGeneradaDto(string? TextoFuente, List<CompromisoActaDto> Compromisos);
