namespace SstControl.Frontend.Shared.Models;

public record SedeDto(int IdSede, int IdEmpresa, string Nombre);
public record EmpresaDto(int IdEmpresa, string Nombre, List<SedeDto> Sedes);
