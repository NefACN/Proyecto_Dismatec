using Microsoft.EntityFrameworkCore;
using proyect.application.Abstractions;
using proyect.application.Dtos;
using proyect.application.Exceptions;
using proyect.domain.Constants;
using proyect.domain.Entidades;

namespace proyect.application.Services;

public sealed class RolesService(IApplicationDbContext db, IUserSessionInvalidator sessions)
{
    public Task<List<RolDto>> ListarAsync(bool? activo, CancellationToken ct) => db.Roles.AsNoTracking()
        .Where(r => !activo.HasValue || r.Activo == activo.Value).OrderBy(r => r.Nombre)
        .Select(r => new RolDto(r.IdRol, r.Nombre, r.Descripcion, r.Activo)).ToListAsync(ct);

    public async Task<RolDto> CrearAsync(CrearRolRequest request, CancellationToken ct)
    {
        var nombre = await ValidarNombreAsync(request.Nombre, null, ct);
        var rol = new Rol { Nombre = nombre, Descripcion = request.Descripcion, Activo = true, FechaCreacion = DateTime.UtcNow };
        db.Roles.Add(rol);
        await db.SaveChangesAsync(ct);
        return new RolDto(rol.IdRol, rol.Nombre, rol.Descripcion, rol.Activo);
    }

    public async Task ActualizarAsync(int id, ActualizarRolRequest request, CancellationToken ct)
    {
        var rol = await BuscarAsync(id, ct);
        if (EsAdmin(rol)) throw new BusinessRuleException("No se puede modificar el rol de Administrador principal.");
        rol.Nombre = await ValidarNombreAsync(request.Nombre, id, ct);
        rol.Descripcion = request.Descripcion;
        rol.FechaModificacion = DateTime.UtcNow;
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.SaveChangesAsync(ct);
        await InvalidarUsuariosAsync(id, ct);
        await transaction.CommitAsync(ct);
    }

    public async Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct)
    {
        var rol = await BuscarAsync(id, ct);
        if (EsAdmin(rol) && !activo) throw new BusinessRuleException("No se puede cambiar el estado del rol Admin.");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        rol.Activo = activo;
        rol.FechaModificacion = DateTime.UtcNow;
        var ahora = DateTime.UtcNow;
        var asignaciones = await db.RolUsuario.Where(ru => ru.IdRol == id && ru.Activo && (ru.FechaFin == null || ru.FechaFin > ahora)).ToListAsync(ct);
        if (!activo)
            foreach (var asignacion in asignaciones)
            {
                asignacion.Activo = false;
                asignacion.FechaFin = ahora;
                asignacion.FechaModificacion = ahora;
            }
        await db.SaveChangesAsync(ct);
        foreach (var userId in asignaciones.Select(a => a.IdUsuario).Distinct()) await sessions.InvalidarAsync(userId);
        await transaction.CommitAsync(ct);
    }

    private async Task InvalidarUsuariosAsync(int rol, CancellationToken ct)
    {
        var ahora = DateTime.UtcNow;
        var ids = await db.RolUsuario.Where(ru => ru.IdRol == rol && ru.Activo && (ru.FechaFin == null || ru.FechaFin > ahora))
            .Select(ru => ru.IdUsuario).Distinct().ToListAsync(ct);
        foreach (var id in ids) await sessions.InvalidarAsync(id);
    }

    private async Task<Rol> BuscarAsync(int id, CancellationToken ct) => await db.Roles.FirstOrDefaultAsync(r => r.IdRol == id, ct)
        ?? throw new BusinessRuleException("El rol indicado no existe.", BusinessError.NotFound);

    private static bool EsAdmin(Rol rol) => rol.Nombre.Equals(RolesSistemas.RolAdmin, StringComparison.OrdinalIgnoreCase);

    private async Task<string> ValidarNombreAsync(string nombre, int? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new BusinessRuleException("El nombre del rol es obligatorio.");
        var normalizado = nombre.Trim();
        if (await db.Roles.AnyAsync(r => (!id.HasValue || r.IdRol != id.Value) && r.Nombre.ToLower() == normalizado.ToLower(), ct))
            throw new BusinessRuleException("Ya existe un rol con ese nombre.", BusinessError.Conflict);
        return normalizado;
    }
}
