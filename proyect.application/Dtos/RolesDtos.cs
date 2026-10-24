using System.ComponentModel.DataAnnotations;

namespace proyect.application.Dtos
{
    public record RolDto(int IdRol, string Nombre, string? Descripcion, bool Activo);

    public record CrearRolRequest(
        [Required, StringLength(50, MinimumLength = 2)] string Nombre,
        [StringLength(255)] string? Descripcion);

    public record ActualizarRolRequest(
        [Required, StringLength(50, MinimumLength = 2)] string Nombre,
        [StringLength(255)] string? Descripcion);

    public record CambiarEstadoRolRequest(bool Activo);
}
