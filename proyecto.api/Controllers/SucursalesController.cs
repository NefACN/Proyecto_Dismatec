using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyect.application.Dtos;
using proyect.application.Services;
using proyect.domain.Constants;

namespace proyecto.api.Controllers;

[ApiController]
[Authorize(Roles = RolesSistemas.RolAdmin)]
[Route("api/sucursales")]
public sealed class SucursalesController(SucursalesService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListarSucursales([FromQuery] bool? activo, CancellationToken ct) => Ok(await service.ListarAsync(activo, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerSucursal(int id, CancellationToken ct)
    {
        var sucursal = await service.ObtenerAsync(id, ct);
        return sucursal is null ? NotFound() : Ok(sucursal);
    }

    [HttpPost]
    public async Task<IActionResult> CrearSucursal(CrearSucursalRequest request, CancellationToken ct)
    {
        var id = await service.CrearAsync(request, ct);
        return CreatedAtAction(nameof(ObtenerSucursal), new { id }, new { IdSucursal = id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarSucursal(int id, ActualizarSucursalRequest request, CancellationToken ct)
    {
        await service.ActualizarAsync(id, request, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoSucursalRequest request, CancellationToken ct)
    {
        await service.CambiarEstadoAsync(id, request.Activo, ct);
        return NoContent();
    }
}
