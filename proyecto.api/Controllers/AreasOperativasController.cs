using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyect.application.Dtos;
using proyect.application.Services;
using proyect.domain.Constants;

namespace proyecto.api.Controllers;

[ApiController]
[Authorize(Roles = RolesSistemas.RolAdmin)]
[Route("api/areas-operativas")]
public sealed class AreasOperativasController(AreasOperativasService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListarAreas(CancellationToken ct) => Ok(await service.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerArea(int id, CancellationToken ct)
    {
        var area = await service.ObtenerAsync(id, ct);
        return area is null ? NotFound() : Ok(area);
    }

    [HttpPost]
    public async Task<IActionResult> CrearArea(CrearAreaOperativaRequest request, CancellationToken ct)
    {
        var id = await service.CrearAsync(request, ct);
        return CreatedAtAction(nameof(ObtenerArea), new { id }, new { IdArea = id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarArea(int id, ActualizarAreaOperativaRequest request, CancellationToken ct)
    {
        await service.ActualizarAsync(id, request, ct);
        return NoContent();
    }
}
