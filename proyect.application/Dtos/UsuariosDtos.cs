using System.ComponentModel.DataAnnotations;

namespace proyect.application.Dtos
{
    public record CrearUsuarioRequest(
        [Required, StringLength(30, MinimumLength = 3)] string NombreUsuario,
        [Required, EmailAddress, StringLength(100)] string Email,
        [Required, StringLength(50)] string Nombre,
        [Required, StringLength(50)] string Apellido,
        [Phone, StringLength(20)] string? Telefono,
        int? IdSucursal,
        int? IdArea,
        int? IdRolInicial);

    public record CrearUsuarioResponse(int IdUsuario, string PasswordTemporal);

    public record UsuarioListItemDto(
        int IdUsuario,
        string NombreUsuario,
        string Email,
        string Nombre,
        string Apellido,
        string? Telefono,
        int? IdSucursal,
        string? Sucursal,
        int? IdArea,
        string? Area,
        decimal ScoreRiesgo,
        bool Activo,
        bool DebeCambiarPassword,
        bool MfaActivo,
        List<string> Roles
    );

    public record RolAsignadoDto(
        int idRol,
        string NombreRol,
        DateTime FechaInicio,
        DateTime? FechaFin,
        bool Activo
    );

    public record UsuarioDetailDto(
        int IdUsuario,
        string NombreUsuario,
        string Email,
        string Nombre,
        string Apellido,
        string? Telefono,
        int? IdSucursal, 
        int? IdArea,     
        bool Activo,
        bool DebeCambiarPassword,
        bool MfaActivo,
        DateTime FechaCreacion,
        List<RolAsignadoDto> Roles
    );

    public record ActualizarUsuarioRequest(
        [Required, StringLength(50)] string Nombre,
        [Required, StringLength(50)] string Apellido,
        [Required, EmailAddress, StringLength(100)] string Email,
        [Phone, StringLength(20)] string? Telefono,
        int? IdSucursal,
        int? IdArea
    );

    public record CambiarEstadoUsuarioRequest(bool Activo);
    public record ResetearPasswordResponse(string PasswordTemporal);

    public record AsignarRolRequest(
        [Range(1, int.MaxValue, ErrorMessage = "IdRol inválido.")] int IdRol,
        DateTime? FechaFin);
}
