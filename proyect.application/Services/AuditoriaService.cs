using proyect.application.Abstractions;

using proyect.domain.Entidades;

namespace proyect.application.Services
{

    public class AuditoriaService : IAuditoriaService
    {
        private readonly IApplicationDbContext _db;
        private readonly IRequestContext _requestContext;

        public AuditoriaService(IApplicationDbContext db, IRequestContext requestContext)
        {
            _db = db;
            _requestContext = requestContext;
        }

        public async Task RegistrarAsync(string accion, bool exitoso, int? idUsuario = null, string? nombreUsuario = null,
            string? entidad = null, string? idEntidadAfectada = null, object? detalles = null)
        {
            var ip = _requestContext.IpAddress;

            _db.RegistrosAuditoria.Add(new RegistroAuditoria
            {
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                Accion = accion,
                Entidad = entidad,
                IdEntidadAfectada = idEntidadAfectada,
                Detalles = detalles is null ? null : System.Text.Json.JsonSerializer.Serialize(detalles),
                IpOrigen = ip,
                Exitoso = exitoso,
                FechaHora = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }
}
