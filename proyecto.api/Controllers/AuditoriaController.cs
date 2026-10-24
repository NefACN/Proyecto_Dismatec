using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyect.application.Dtos;
using proyect.application.Services;
using proyect.domain.Constants;

namespace proyecto.api.Controllers;

[ApiController]
[Authorize(Roles = RolesSistemas.RolAdmin)]
[Route("api/auditoria")]
public sealed class AuditoriaController(AuditoriaConsultaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? idUsuario, [FromQuery] string? accion,
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] int pagina = 1, [FromQuery] int tamanioPagina = 50, CancellationToken ct = default)
        => Ok(await service.ListarAsync(idUsuario, accion, desde, hasta, pagina, tamanioPagina, ct));
}
