using Microsoft.EntityFrameworkCore;
using proyect.application.Abstractions;
using proyect.application.Dtos;

namespace proyect.application.Services;

public sealed class CiudadesService(IApplicationDbContext db)
{
    public Task<List<CiudadDto>> ListarAsync(CancellationToken ct) => db.Ciudades.AsNoTracking().OrderBy(c => c.Nombre)
        .Select(c => new CiudadDto(c.Id_Ciudad, c.Nombre)).ToListAsync(ct);

    // Preserve the existing endpoint's array-of-names response.
    public Task<List<string>> ObtenerNombresAsync(int id, CancellationToken ct) => db.Ciudades.AsNoTracking()
        .Where(c => c.Id_Ciudad == id).Select(c => c.Nombre).ToListAsync(ct);
}
