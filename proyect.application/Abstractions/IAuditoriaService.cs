using proyect.domain.Entidades;

namespace proyect.application.Abstractions
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(string accion, bool exitoso, int? idUsuario = null, string? nombreUsuario = null,
        string? entidad = null, string? idEntidadAfectada = null, object? detalles = null);
    }
}
