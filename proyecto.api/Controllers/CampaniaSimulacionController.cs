using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyect.application.Abstractions;
using proyect.domain.Entidades;
using proyect.application.Dtos;
using proyect.application.Security;
using proyect.domain.Constants;
using proyect.application;
using System.Security.Claims;

namespace proyecto.api.Controllers
{
    [ApiController]
    [Route("api/campanias-phishing")]
    [Authorize(Roles = RolesSistemas.RolAdmin)]
    public class CampaniaSimulacionController : ControllerBase
    {
        private readonly IApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public CampaniaSimulacionController(IApplicationDbContext db, IEmailSender emailSender, IConfiguration configuration)
        {
            _db = db;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpGet("plantillas")]
        public async Task<ActionResult<List<PlantillaDto>>> ListarPlantillas()
        {
            var plantillas = await _db.PlantillasPhishing
                .Include(p => p.Criticidad)
                .AsNoTracking()
                .Select(p => new PlantillaDto
                (
                    p.Id_Plantilla,
                    p.Nombre,
                    p.AsuntoCorreo,
                    p.Criticidad.Nivel
                ))
                .ToListAsync();
            return Ok(plantillas);
        }

        [HttpGet("plantillas/{id:int}")]
        public async Task<IActionResult> ObtenerPlantilla(int id)
        {
            var plantilla = await _db.PlantillasPhishing
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id_Plantilla == id);

            if (plantilla == null) return NotFound(new { mensaje = "La plantilla no existe." });

            return Ok(new
            {
                idPlantilla = plantilla.Id_Plantilla,
                nombre = plantilla.Nombre,
                asuntoCorreo = plantilla.AsuntoCorreo,
                cuerpoHtml = plantilla.CuerpoHtml,
                idCriticidad = plantilla.Id_Criticidad,
                idCategoriaPregunta = plantilla.Id_CategoriaPegunta,
            });
        }

        [HttpPut("plantillas/{id:int}")]
        public async Task<IActionResult> ActualizarPlantilla(int id, [FromBody] CrearPlantillaRequest request)
        {
            var plantilla = await _db.PlantillasPhishing.FirstOrDefaultAsync(p => p.Id_Plantilla == id);
            if (plantilla == null) return NotFound(new { mensaje = "Plantilla no encontrada." });

            var existeCriticidad = await _db.NivelesCriticidad.AnyAsync(c => c.Id_NivelCriticidad == request.IdCriticidad);
            if (!existeCriticidad)
                return BadRequest(new { mensaje = "El nivel de criticidad seleccionado no existe." });

            var existeCategoria = await _db.CategoriasPregunta.AnyAsync(c => c.IdCategoria == request.IdCategoriaPregunta);
            if (!existeCategoria)
                return BadRequest(new { mensaje = "La categoría de concientización seleccionada no existe." });

            plantilla.Nombre = request.Nombre;
            plantilla.AsuntoCorreo = request.AsuntoCorreo;
            plantilla.CuerpoHtml = request.CuerpoHtml;
            plantilla.Id_Criticidad = request.IdCriticidad;
            plantilla.Id_CategoriaPegunta = request.IdCategoriaPregunta;

            _db.PlantillasPhishing.Update(plantilla);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("plantillas/{id:int}")]
        public async Task<IActionResult> EliminarPlantilla(int id)
        {
            var plantilla = await _db.PlantillasPhishing.FindAsync(id);
            if (plantilla == null) return NotFound(new { mensaje = "La plantilla no existe." });

            
            var vinculadaACampania = await _db.CampaniasSimulacion.AnyAsync(c => c.Id_Plantilla == id);
            if (vinculadaACampania)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la plantilla porque está vinculada a una o más campañas de simulación." });
            }

            try
            {
                _db.PlantillasPhishing.Remove(plantilla);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { mensaje = "No se pudo eliminar la plantilla por restricciones de integridad referencial." });
            }
        }

        [HttpPost("plantillas")]
        public async Task<IActionResult> CrearPlantilla(CrearPlantillaRequest request)
        {
            var existeCriticidad = await _db.NivelesCriticidad.AnyAsync(c => c.Id_NivelCriticidad == request.IdCriticidad);
            if (!existeCriticidad)
                return BadRequest(new { mensaje = "El nivel de criticidad seleccionado no existe." });
            var existeCategoria = await _db.CategoriasPregunta.AnyAsync(c => c.IdCategoria == request.IdCategoriaPregunta);
            if (!existeCategoria)
                return BadRequest(new { mensaje = "La categoría de concientización seleccionada no existe." });

            var plantilla = new PlantillaPhishing
            {
                Nombre = request.Nombre,
                AsuntoCorreo = request.AsuntoCorreo,
                CuerpoHtml = request.CuerpoHtml,
                Id_Criticidad = request.IdCriticidad,
                Id_CategoriaPegunta = request.IdCategoriaPregunta,
            };
            _db.PlantillasPhishing.Add(plantilla);
            await _db.SaveChangesAsync();
            return Ok(new { id = plantilla.Id_Plantilla, mensaje = "Plantilla creada exitosamente" });
        }

        [HttpGet("categorias-educativas")]
        [Authorize(Roles = RolesSistemas.RolAdmin)] 
        public async Task<IActionResult> ObtenerCategoriasEducativas()
        {
            var categorias = await _db.CategoriasPregunta
                .AsNoTracking()
                .Select(c => new {
                    idCategoria = c.IdCategoria,
                    nombre = c.Nombre
                })
                .ToListAsync();

            return Ok(categorias);
        }

        [HttpGet]
        public async Task<ActionResult<List<CampaniaListItemDto>>> ListarCampanias()
        {
            var campanias = await _db.CampaniasSimulacion
                .Include(c => c.Plantilla)
                .Include(c => c.Estado)
                .Include(c => c.Segmentaciones)
                .AsNoTracking()
                .OrderByDescending(c => c.Id_Campania)
                .Select(c => new CampaniaListItemDto(
                    c.Id_Campania,
                    c.Titulo,
                    c.Plantilla.Nombre,
                    c.Estado.NombreEstado,
                    c.FechaInicioProgramada,
                    c.FechaFinProgramada,
                    c.Segmentaciones.Count 
                ))
                .ToListAsync();

            return Ok(campanias);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CampaniaDetalleDto>> ObtenerDetalle(int id)
        {
            var campania = await _db.CampaniasSimulacion
                .Include(c => c.Plantilla)
                .Include(c => c.Estado)
                .Include(c => c.Creador)
                .Include(c => c.Segmentaciones).ThenInclude(s => s.AreaOperativa)
                .Include(c => c.Envios)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id_Campania == id);

            if (campania is null) return NotFound();

            var detalle = new CampaniaDetalleDto(
                campania.Id_Campania,
                campania.Titulo,
                campania.Plantilla.Nombre,
                campania.Estado.NombreEstado,
                $"{campania.Creador.Nombre} {campania.Creador.Apellido}",
                campania.FechaInicioProgramada,
                campania.FechaFinProgramada,
                campania.Segmentaciones.Select(s => s.AreaOperativa.Nombre).ToList(),
                campania.Envios.Count,
                campania.Envios.Count(e => e.FueAbierto),
                campania.Envios.Count(e => e.HizoClick)
            );

            return Ok(detalle);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCampania([FromBody] CrearCampaniaRequest request)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var idAdmin = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var campania = new CampaniaSimulacion
                {
                    Titulo = request.Titulo,
                    Id_Plantilla = request.IdPlantilla,
                    Id_Estado = 1,
                    CreadoPor = idAdmin,
                    FechaInicioProgramada = request.FechaInicioProgramada,
                    FechaFinProgramada = request.FechaFinProgramada
                };

                _db.CampaniasSimulacion.Add(campania);
                await _db.SaveChangesAsync(); 

                foreach (var idArea in request.AreasSeleccionadas)
                {
                    _db.SegmentacionesCampaniaArea.Add(new SegmentacionCampaniArea
                    {
                        Id_Campania = campania.Id_Campania,
                        Id_Area = idArea
                    });
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { idCampania = campania.Id_Campania });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { mensaje = "Error al crear la campaña segmentada." });
            }
        }

        [HttpPost("{id:int}/ejecutar")]
        public async Task<IActionResult> EjecutarCampania(int id)
        {
            var campania = await _db.CampaniasSimulacion
                 .Include(c => c.Segmentaciones)
                 .Include(c => c.Plantilla)
                 .FirstOrDefaultAsync(c => c.Id_Campania == id);
             
            if (campania == null) return NotFound("Campaña no encontrada.");
            
            if (campania.Id_Estado != 1) // Asumiendo que 1 es 'Programada/Lista' y 2 es 'En Ejecución'
                return BadRequest("La campaña ya fue ejecutada o se encuentra finalizada.");

            if (campania.FechaInicioProgramada.HasValue && campania.FechaInicioProgramada.Value > DateTime.UtcNow)
                return BadRequest($"No se puede ejecutar la campaña. Está programada para iniciar el {campania.FechaInicioProgramada.Value:dd/MM/yyyy}.");

            if (campania.FechaFinProgramada.HasValue && campania.FechaFinProgramada.Value < DateTime.UtcNow)
                return BadRequest("No se puede ejecutar la campaña porque su fecha de finalización ya expiró.");
            
            var idsAreasObjetivo = campania.Segmentaciones.Select(s => s.Id_Area).ToList();

            if (!idsAreasObjetivo.Any())
                return BadRequest("La campaña no tiene áreas segmentadas.");

            var usuariosBlanco = await _db.Users
                .Where(u => u.Activo && u.Id_Area != null && idsAreasObjetivo.Contains(u.Id_Area.Value))
                .ToListAsync();

            if (!usuariosBlanco.Any())
                return BadRequest("No se encontraron usuarios activos en las áreas segmentadas.");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var enviosGenerados = 0;

            foreach (var victima in usuariosBlanco)
            {
                if (string.IsNullOrWhiteSpace(victima.Email)) continue;
                var yaEnviado = await _db.EnviosSimulacion
                    .AnyAsync(e => e.Id_Campania == campania.Id_Campania && e.Id_Usuario == victima.Id);

                if (!yaEnviado)
                {
                    var tokenUnico = Guid.NewGuid();

                    _db.EnviosSimulacion.Add(new EnvioSimulacion
                    {
                        Id_Campania = campania.Id_Campania,
                        Id_Usuario = victima.Id,
                        TokenUuid = tokenUnico,
                    });

                 
                    var linkTrampa = $"{baseUrl}/api/campanias-phishing/click/{tokenUnico}";
                    var linkPixel = $"{baseUrl}/api/campanias-phishing/track/{tokenUnico}";
                    var pixelHtml = $"<img src=\"{linkPixel}\" width=\"1\" height=\"1\" style=\"display:none;\" />";

                 
                    var cuerpoPersonalizado = campania.Plantilla.CuerpoHtml
                        .Replace("{{NombreUsuario}}", victima.Nombre) 
                        .Replace("{{LinkTrampa}}", linkTrampa)        
                        + pixelHtml;                                  

                    
                    
                    await _emailSender.EnviarAsync(victima.Email, $"{victima.Nombre} {victima.Apellido}", campania.Plantilla.AsuntoCorreo, cuerpoPersonalizado);

                    enviosGenerados++;
                }
            }

            campania.Id_Estado = 2;
            _db.CampaniasSimulacion.Update(campania);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Motor de simulación ejecutado.",
                victimasSeleccionadas = enviosGenerados
            });
        }

        [HttpGet("track/{token}")]
        [AllowAnonymous] 
        public async Task<IActionResult> RastrearApertura(Guid token)
        {
            var envio = await _db.EnviosSimulacion.FirstOrDefaultAsync(e => e.TokenUuid == token);

           
            if (envio != null && !envio.FueAbierto)
            {
                envio.FueAbierto = true;
                await _db.SaveChangesAsync();
            }

            byte[] pixelInvisible = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            return File(pixelInvisible, "image/gif");
        }

        [HttpGet("click/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> RastrearClick(Guid token)
        {
            var envio = await _db.EnviosSimulacion.FirstOrDefaultAsync(e => e.TokenUuid == token);

            if (envio != null)
            {
                // Solo penalizar si es la primera vez que hace clic
                if (!envio.HizoClick)
                {
                    envio.HizoClick = true;
                    envio.FueAbierto = true;
                    envio.FechaVulneracion = DateTime.UtcNow;

                    var victima = await _db.Users.FindAsync(envio.Id_Usuario);
                    if (victima != null)
                    {
                        victima.CyberScore -= 10;
                        if (victima.CyberScore < 0) victima.CyberScore = 0;

                        _db.Users.Update(victima); // Actualizamos a la víctima
                    }

                    // ❌ AQUÍ BORRAMOS LA LÍNEA QUE DECÍA: _db.Users.Update(envio.Usuario);

                    await _db.SaveChangesAsync(); // Guardamos los cambios
                }
            }

            // 🟢 Asegúrate de usar la URL que pusiste en appsettings.json
            var frontBaseUrl = _configuration["Frontend:BaseUrl"] ?? "https://localhost:44320";

            return Redirect($"{frontBaseUrl}/phishing-educacion.html?token={token}");
        }

        [HttpGet("criticidades")]
        public async Task<IActionResult> ObtenerCriticidades()
        {
            // Leemos la tabla NivelesCriticidad directo de la BD
            var criticidades = await _db.NivelesCriticidad
                .AsNoTracking()
                .Select(c => new {
                    idCriticidad = c.Id_NivelCriticidad,
                    nivel = c.Nivel
                })
                .ToListAsync();

            return Ok(criticidades);
        }
    }
}
