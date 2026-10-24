using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyect.application.Dtos;
using proyect.application.Services;
using proyect.domain.Constants;

namespace proyecto.api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = RolesSistemas.RolAdmin)]
public sealed class RolesController(RolesService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListarRoles([FromQuery] bool? activo, CancellationToken ct) => Ok(await service.ListarAsync(activo, ct));

    [HttpPost]
    public async Task<IActionResult> CrearRol(CrearRolRequest request, CancellationToken ct) => Ok(await service.CrearAsync(request, ct));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarRol(int id, ActualizarRolRequest request, CancellationToken ct)
    {
        await service.ActualizarAsync(id, request, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoRolRequest request, CancellationToken ct)
    {
        await service.CambiarEstadoAsync(id, request.Activo, ct);
        return NoContent();
    }
}
