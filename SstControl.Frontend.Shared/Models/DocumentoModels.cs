namespace SstControl.Frontend.Shared.Models;

public record DocumentoDto(int IdDocumento, string NombreTipo, string NombreColaborador, string Actividad,
    DateOnly FechaCaptura, DateOnly FechaVencimiento, string Estado, string? NombreQuienAprueba);

public record CrearDocumentoDto(int IdTipoDocumento, string NombreColaborador, string Actividad, DateOnly FechaVencimiento,
    int? IdEmpresa, int? IdSede);

/// <summary>Conteos agregados del ciclo documental, calculados en el servidor —
/// espejo de ResumenDocumentosDto en el backend.</summary>
public record ResumenDocumentosDto(int Total, int Pendientes, int Vencidos, int Aprobados);

/// <summary>Resultado de escanear un documento físico — el insumo digital (texto
/// reconocido por OCR) que queda asociado al Documento. Espejo de
/// DigitalizacionDocumentoDto en el backend.</summary>
public record DigitalizacionDocumentoDto(int IdDocumento, string NombreArchivoOriginal, string TipoContenido,
    long TamanioBytes, string? TextoExtraido, double? Confianza, DateTimeOffset FechaEscaneo);
