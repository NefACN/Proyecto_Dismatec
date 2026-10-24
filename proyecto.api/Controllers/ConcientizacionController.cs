using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyect.application.Abstractions;
using Microsoft.EntityFrameworkCore;
using proyect.application.Dtos;
using proyect.domain.Entidades;

namespace proyecto.api.Controllers
{
    [ApiController]
    [Route("api/concientizacion")]
    public class ConcientizacionController : ControllerBase
    {
        private readonly IApplicationDbContext _db;

        public ConcientizacionController(IApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("rastrear/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> RastrearYObtenerPreguntas(Guid token)
        {
            var envio = await _db.EnviosSimulacion
                .Include(e => e.Campania)
                    .ThenInclude(c => c.Plantilla)
                .FirstOrDefaultAsync(e => e.TokenUuid == token);
            
            if (envio == null) return NotFound(new { mensaje = "Token inválido o expirado." });

            if (envio.Campania.Id_Estado != 2 || (envio.Campania.FechaFinProgramada.HasValue && envio.Campania.FechaFinProgramada.Value < DateTime.UtcNow))
            {
                return BadRequest(new {
                    mensaje = "esta simulacion de phishing ya ha concluido, Gracias por tu interes en ciberseguridad",
                    campaniaInactiva = true,
                });
            }

            if (!envio.HizoClick)
            {
                envio.HizoClick = true;
                envio.FueAbierto = true;
                envio.FechaVulneracion = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            var preguntasYaAsignadas = await _db.PreguntasAsignadasEnvio
                .Include(pa => pa.Pregunta)
                .ThenInclude(p => p.Opciones)
                .Where(pa => pa.Id_Envio == envio.Id_Envio)
                .Select(pa => pa.Pregunta)
                .ToListAsync();

            List<PreguntaConcientizacion> preguntasMostrar;

            if (preguntasYaAsignadas.Any())
            {
                preguntasMostrar = preguntasYaAsignadas;
            }
            else
            {
                int idCategoriaRequerida = envio.Campania.Plantilla.Id_CategoriaPegunta;
                int totalPreguntas = await _db.PregutasConcientizacion
                    .Where(p => p.Id_Categoria == idCategoriaRequerida)
                    .CountAsync();

                if (totalPreguntas == 0)
                {
                    return BadRequest("No hay preguntas configuradas");
                }

                preguntasMostrar = new List<PreguntaConcientizacion>();
                Random rand = new Random();
                int lim = Math.Min(2, totalPreguntas);
                var indSelect = new HashSet<int>();
                while (indSelect.Count < lim)
                {
                    int randIndx = rand.Next(0, totalPreguntas);
                    if (indSelect.Add(randIndx))
                    {
                        var pregunta = await _db.PregutasConcientizacion
                            .Include(p => p.Opciones)
                            .Where(p => p.Id_Categoria == envio.Campania.Plantilla.Id_CategoriaPegunta)
                            .Skip(randIndx)
                            .Take(1)
                            .FirstOrDefaultAsync();
                        if (pregunta != null)
                        {
                            preguntasMostrar.Add(pregunta);
                        }
                    }
                }
                    

                foreach (var preg in preguntasMostrar)
                {
                    _db.PreguntasAsignadasEnvio.Add(new PreguntaAsignadaEnvio
                    {
                        Id_Envio = envio.Id_Envio,
                        Id_Pregunta = preg.Id_Pregunta,
                    });
                }
                await _db.SaveChangesAsync();
            }

            var preguntasDto = preguntasMostrar.Select(p => new PreguntaDto(
                p.Id_Pregunta,
                p.TextoPregunta,
                p.ExplicacionEducativa,
                p.Opciones.Select(o => new OpcionDto(o.Id_Opcion, o.TextoOpcion)).ToList()
                )).ToList();

            return Ok(new
            {
                cuerpoOriginal = envio.Campania.Plantilla.CuerpoHtml,
                preguntas = preguntasDto
            });
        }

        [HttpPost("completar/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> CompletarEducacion(Guid token, [FromBody] ValidarRespuestasRequest request)
        {
            if (request.IdsOpcionesSeleccionadas == null || !request.IdsOpcionesSeleccionadas.Any())
                return BadRequest(new { mensaje = "Debe responder todas las preguntas." });

            var envio = await _db.EnviosSimulacion
                .Include(e => e.Usuario)
                .Include(e => e.Campania)
                .FirstOrDefaultAsync(e => e.TokenUuid == token);

            if (envio == null) return NotFound(new { mensaje = "Token inválido." });
            if (envio.CapacitacionCompleta) return BadRequest(new { mensaje = "Ya has completado esta capacitación." });

            var opcionesCorrectasEsperadas = await _db.PreguntasAsignadasEnvio
                .Where(pa => pa.Id_Envio == envio.Id_Envio)
                .SelectMany(pa => pa.Pregunta.Opciones)
                .Where(o => o.EsCorrecta)
                .Select(o => o.Id_Opcion)
                .ToListAsync();

            bool esCorrecto = request.IdsOpcionesSeleccionadas.Count == opcionesCorrectasEsperadas.Count &&
                              request.IdsOpcionesSeleccionadas.All(id => opcionesCorrectasEsperadas.Contains(id));

            if (!esCorrecto)
                return BadRequest(new { mensaje = "Algunas respuestas son incorrectas. Revisa la explicación e intenta de nuevo." });

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                envio.CapacitacionCompleta = true;
                envio.FechaCapacitacion = DateTime.UtcNow;

                int puntosBase = 25;
                int puntosGanados = puntosBase * envio.Campania.NivelDificultad;

                envio.PuntosRecuperados = puntosGanados;
                envio.Usuario.CyberScore += puntosGanados;
                _db.Users.Update(envio.Usuario);
                await _db.SaveChangesAsync(); 

                var insigniasNuevas = await _db.Insignias
                    .Where(i => i.PuntosRequeridos <= envio.Usuario.CyberScore &&
                                !_db.LogrosUsuario.Any(lu => lu.Id_Usuario == envio.Usuario.Id && lu.Id_Insignia == i.Id_Insignia))
                    .ToListAsync();

                var nombresInsigniasGanadas = new List<string>();

                if (insigniasNuevas.Any())
                {
                    foreach (var insignia in insigniasNuevas)
                    {
                        _db.LogrosUsuario.Add(new LogroUsuario
                        {
                            Id_Usuario = envio.Usuario.Id,
                            Id_Insignia = insignia.Id_Insignia,
                            FechaObtencion = DateTime.UtcNow
                        });
                        nombresInsigniasGanadas.Add(insignia.Nombre);
                    }
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "¡Felicidades! Has completado tu capacitación.",
                    puntosGanados = puntosGanados,
                    insigniasDesbloqueadas = nombresInsigniasGanadas
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al procesar la gamificación." });
            }
        }
    }
}
