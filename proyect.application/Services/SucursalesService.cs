using Microsoft.EntityFrameworkCore;
using proyect.application.Abstractions;
using proyect.application.Dtos;
using proyect.application.Exceptions;
using proyect.domain.Entidades;

namespace proyect.application.Services;

public sealed class SucursalesService(IApplicationDbContext db)
{
    private static IQueryable<SucursalDto> Proyectar(IQueryable<Sucursal> query) => query.OrderBy(s => s.Nombre).Select(s => new SucursalDto(
        s.Id_Sucursal, s.Nombre, s.Id_Ciudad, s.Ciudad.Nombre, s.Direccion, s.Id_GerenteResponsable,
        s.GerenteResponsable == null ? null : s.GerenteResponsable.Nombre + " " + s.GerenteResponsable.Apellido, s.Activo));

    public Task<List<SucursalDto>> ListarAsync(bool? activo, CancellationToken ct) =>
        Proyectar(db.Sucursales.AsNoTracking().Where(s => !activo.HasValue || s.Activo == activo.Value)).ToListAsync(ct);

    public Task<SucursalDto?> ObtenerAsync(int id, CancellationToken ct) => Proyectar(db.Sucursales.AsNoTracking().Where(s => s.Id_Sucursal == id)).FirstOrDefaultAsync(ct);

    public async Task<int> CrearAsync(CrearSucursalRequest request, CancellationToken ct)
    {
        await ValidarAsync(request.Nombre, request.IdCiudad, request.IdGerenteResponsable, ct);
        var sucursal = new Sucursal { Nombre = request.Nombre.Trim(), Id_Ciudad = request.IdCiudad, Direccion = request.Direccion, Id_GerenteResponsable = request.IdGerenteResponsable };
        db.Sucursales.Add(sucursal);
        await db.SaveChangesAsync(ct);
        return sucursal.Id_Sucursal;
    }

    public async Task ActualizarAsync(int id, ActualizarSucursalRequest request, CancellationToken ct)
    {
        var sucursal = await BuscarAsync(id, ct);
        await ValidarAsync(request.Nombre, request.IdCiudad, request.IdGerenteResponsable, ct);
        sucursal.Nombre = request.Nombre.Trim();
        sucursal.Id_Ciudad = request.IdCiudad;
        sucursal.Direccion = request.Direccion;
        sucursal.Id_GerenteResponsable = request.IdGerenteResponsable;
        await db.SaveChangesAsync(ct);
    }

    public async Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct)
    {
        (await BuscarAsync(id, ct)).Activo = activo;
        await db.SaveChangesAsync(ct);
    }

    private async Task<Sucursal> BuscarAsync(int id, CancellationToken ct) => await db.Sucursales.FirstOrDefaultAsync(s => s.Id_Sucursal == id, ct)
        ?? throw new BusinessRuleException("La sucursal indicada no existe.", BusinessError.NotFound);

    private async Task ValidarAsync(string nombre, int ciudad, int? gerente, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new BusinessRuleException("El nombre de la sucursal es obligatorio.");
        if (!await db.Ciudades.AnyAsync(c => c.Id_Ciudad == ciudad, ct)) throw new BusinessRuleException("La ciudad indicada no existe.");
        if (gerente.HasValue && !await db.Users.AnyAsync(u => u.Id == gerente.Value, ct))
            throw new BusinessRuleException("El usuario asignado como gerente no existe.");
    }
}
