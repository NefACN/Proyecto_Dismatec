using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyect.application.Dtos;
using proyect.application.Services;
using proyect.domain.Constants;

namespace proyecto.api.Controllers;

[ApiController]
[Authorize(Roles = RolesSistemas.RolAdmin)]
[Route("api/ciudades")]
public sealed class CiudadesController(CiudadesService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListarCiudades(CancellationToken ct) => Ok(await service.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerById(int id, CancellationToken ct) => Ok(await service.ObtenerNombresAsync(id, ct));
}
