namespace SstControl.Frontend.Shared.Models;

public record DocumentoDto(int IdDocumento, string NombreTipo, string NombreColaborador, string Actividad,
    DateOnly FechaCaptura, DateOnly FechaVencimiento, string Estado, string? NombreQuienAprueba);

public record CrearDocumentoDto(int IdTipoDocumento, string NombreColaborador, string Actividad, DateOnly FechaVencimiento,
    int? IdEmpresa, int? IdSede);
