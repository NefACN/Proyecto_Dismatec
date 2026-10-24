using System.ComponentModel.DataAnnotations;

namespace proyect.application.Dtos
{
    public record SucursalDto(
        int IdSucursal,
        string Nombre,
        int IdCiudad,
        string NombreCiudad,
        string? Direccion,
        int? IdGerenteResponsable,
        string? NombreGerente,
        bool Activo
    );

    public record CrearSucursalRequest(
        [Required, StringLength(50)] string Nombre,
        [Range(1, int.MaxValue)] int IdCiudad,
        [StringLength(100)] string? Direccion,
        [Range(1, int.MaxValue)] int? IdGerenteResponsable
    );

    public record ActualizarSucursalRequest(
        [Required, StringLength(50)] string Nombre,
        [Range(1, int.MaxValue)] int IdCiudad,
        [StringLength(100)] string? Direccion,
        [Range(1, int.MaxValue)] int? IdGerenteResponsable
    );

    public record CambiarEstadoSucursalRequest(bool Activo);
}
