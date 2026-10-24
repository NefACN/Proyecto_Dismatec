namespace proyect.application.Dtos
{
    public record RegistroAuditoriaDto(long Id, DateTime FechaHora, int? IdUsuario, string? NombreUsuario,
    string Accion, string? Entidad, string? IdEntidadAfectada, bool Exitoso, string? IpOrigen, string? Detalles);
}
