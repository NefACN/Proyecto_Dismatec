using Microsoft.EntityFrameworkCore;
using proyect.application.Abstractions;
using proyect.application.Dtos;
using proyect.application.Exceptions;

namespace proyect.application.Services;

public sealed class AuditoriaConsultaService(IApplicationDbContext db)
{
    public async Task<List<RegistroAuditoriaDto>> ListarAsync(int? idUsuario, string? accion, DateTime? desde, DateTime? hasta,
        int pagina, int tamanioPagina, CancellationToken ct)
    {
        tamanioPagina = Math.Clamp(tamanioPagina, 1, 200);
        if (pagina < 1 || pagina > int.MaxValue / tamanioPagina)
            throw new BusinessRuleException("La página solicitada está fuera del rango permitido.");
        if (desde.HasValue) desde = desde.Value.ToUniversalTime();
        if (hasta.HasValue) hasta = hasta.Value.ToUniversalTime();
        if (desde > hasta) throw new BusinessRuleException("El inicio del periodo no puede ser posterior al final.");
        var query = db.RegistrosAuditoria.AsNoTracking();
        if (idUsuario.HasValue) query = query.Where(r => r.IdUsuario == idUsuario);
        if (!string.IsNullOrWhiteSpace(accion)) query = query.Where(r => r.Accion == accion);
        if (desde.HasValue) query = query.Where(r => r.FechaHora >= desde);
        if (hasta.HasValue) query = query.Where(r => r.FechaHora <= hasta);
        return await query.OrderByDescending(r => r.FechaHora).ThenByDescending(r => r.Id_Auditoria)
            .Skip((pagina - 1) * tamanioPagina).Take(tamanioPagina)
            .Select(r => new RegistroAuditoriaDto(r.Id_Auditoria, r.FechaHora, r.IdUsuario, r.NombreUsuario,
                r.Accion, r.Entidad, r.IdEntidadAfectada, r.Exitoso, r.IpOrigen, r.Detalles)).ToListAsync(ct);
    }
}
