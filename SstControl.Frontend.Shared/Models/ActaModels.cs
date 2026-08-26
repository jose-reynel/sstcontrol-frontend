namespace SstControl.Frontend.Shared.Models;

public record ActaDto(int IdActa, int IdEmpresa, int IdSede, string Tipo, string Titulo, DateOnly Fecha,
    string? Asistentes, string? Notas, string NombreCreador);

public record CrearActaDto(int IdEmpresa, int IdSede, string Tipo, string Titulo, DateOnly Fecha,
    string? Asistentes, string? Notas);
