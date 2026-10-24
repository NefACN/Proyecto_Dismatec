using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyect.application.Abstractions;
using proyect.domain.Entidades;
using proyect.application.Dtos;
using System.Security.Claims;

namespace proyecto.api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardUsuarioController : ControllerBase
    {
        private readonly IApplicationDbContext _db;

        public DashboardUsuarioController(IApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("mi-perfil")]
        public async Task<IActionResult> ObtenerMiPerfil()
        {
            // Extraer el ID del usuario del token JWT actual
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out int idUsuario)) return Unauthorized();

            var perfil = await _db.Users
                .Include(u => u.Sucursal)
                .Include(u => u.LogrosObtenidos)
                    .ThenInclude(l => l.Insignia)
                .AsNoTracking()
                .Where(u => u.Id == idUsuario)
                .Select(u => new
                {
                    nombre = u.Nombre,
                    apellido = u.Apellido,
                    sucursal = u.Sucursal == null ? "Sin sucursal" : u.Sucursal.Nombre,
                    cyberScore = u.CyberScore,
                    scoreRiesgo = u.ScoreRiesgo,
                    insigniasObtenidas = u.LogrosObtenidos.Select(l => new
                    {
                        nombre = l.Insignia.Nombre,
                        icono = l.Insignia.IconoSvg,
                        fecha = l.FechaObtencion
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (perfil == null) return NotFound();

            return Ok(perfil);
        }

        [HttpGet("ranking-sucursales")]
        public async Task<IActionResult> ObtenerRankingSucursales()
        {
            // Gamificación a nivel organizacional
            var ranking = await _db.Users
                .Where(u => u.Activo && u.Sucursal != null)
                .GroupBy(u => new { u.Id_Sucursal, u.Sucursal!.Nombre })
                .Select(g => new
                {
                    sucursal = g.Key.Nombre,
                    promedioCyberScore = g.Average(u => u.CyberScore),
                    totalEmpleados = g.Count()
                })
                .OrderByDescending(r => r.promedioCyberScore)
                .ToListAsync();

            return Ok(ranking);
        }

        [HttpGet("mi-historial")]
        public async Task<IActionResult> ObtenerMiHistorial()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out int idUsuario)) return Unauthorized();

            var historial = await _db.EnviosSimulacion
                .Include(e => e.Campania)
                .ThenInclude(c => c.Plantilla)
                .Where(e => e.Id_Usuario == idUsuario && e.CapacitacionCompleta)
                .OrderByDescending(e => e.FechaCapacitacion)
                .Select(e => new
                {
                    nombreCampania = e.Campania.Titulo,
                    temaAprendido = e.Campania.Plantilla.Nombre,
                    fechaCompletado = e.FechaCapacitacion,
                    puntosObtenidos = e.PuntosRecuperados,
                })
                .ToListAsync();
            return Ok(historial);
        }

        [HttpGet("cursos-disponibles")]
        public async Task<IActionResult> ObtenerCursosDisponibles()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var categorias = await _db.CategoriasPregunta
                .Select(c => new
                {
                    idCategoria = c.IdCategoria,
                    nombre = c.Nombre,
                    ultimoExito = _db.IntentoCursoVoluntario
                        .Where(i => i.Id_Usuario == idUsuario && i.Id_Categoria == c.IdCategoria && i.Exitoso == true)
                        .OrderByDescending(i => i.FechaCompletado)
                        .Select(i => i.FechaCompletado)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Cooldown dias ....
            int diasCooldown = 30;

            var resultado = categorias.Select(c => new
            {
                c.idCategoria,
                c.nombre,
                c.ultimoExito,
                otorgaPuntos = c.ultimoExito == null || (DateTime.UtcNow - c.ultimoExito.Value).TotalDays > diasCooldown,
                mensajeCooldown = c.ultimoExito != null && (DateTime.UtcNow - c.ultimoExito.Value).TotalDays <= diasCooldown
                    ? $"Repaso habilitado. Podrás ganar puntos por esta categoría de nuevo en {diasCooldown - Math.Floor((DateTime.UtcNow - c.ultimoExito.Value).TotalDays)} días."
                    : "¡Completa este curso para ganar puntos de inmunidad!"
            });

            return Ok(resultado);
        }

        [HttpPost("cursos/{idCategoria}/iniciar")]
        public async Task<IActionResult> IniciarCurso(int idCategoria)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (!await _db.CategoriasPregunta.AnyAsync(c => c.IdCategoria == idCategoria))
                return NotFound(new { mensaje = "La categoría no existe." });

            var intentoEnCurso = await _db.IntentoCursoVoluntario
                .Include(i => i.PreguntasAsignadas)
                    .ThenInclude(pa => pa.Pregunta)
                        .ThenInclude(p => p.Opciones)
                .FirstOrDefaultAsync(i => i.Id_Usuario == idUsuario && i.Id_Categoria == idCategoria && i.FechaCompletado == null);

            List<PreguntaConcientizacion> preguntasMostrar = new();

            if (intentoEnCurso != null)
            {
                preguntasMostrar = intentoEnCurso.PreguntasAsignadas.Select(pa => pa.Pregunta).ToList();
            }
            else
            {
                intentoEnCurso = new IntentoCursoVoluntario
                {
                    Id_Usuario = idUsuario,
                    Id_Categoria = idCategoria,
                    FechaInicio = DateTime.UtcNow
                };
                _db.IntentoCursoVoluntario.Add(intentoEnCurso);
                await _db.SaveChangesAsync(); 

                int totalPreguntas = await _db.PregutasConcientizacion.CountAsync(p => p.Id_Categoria == idCategoria);
                if (totalPreguntas == 0) return BadRequest("No hay preguntas para esta categoría.");

                Random rand = new Random();
                int limite = Math.Min(2, totalPreguntas);
                var indices = new HashSet<int>();

                while (indices.Count < limite)
                {
                    int randomIndex = rand.Next(0, totalPreguntas);
                    if (indices.Add(randomIndex))
                    {
                        var pregunta = await _db.PregutasConcientizacion
                            .Include(p => p.Opciones)
                            .Where(p => p.Id_Categoria == idCategoria)
                            .Skip(randomIndex)
                            .Take(1)
                            .FirstOrDefaultAsync();

                        if (pregunta != null)
                        {
                            preguntasMostrar.Add(pregunta);
                            _db.PreguntaAsginadasCurso.Add(new PreguntaAsignadaCurso
                            {
                                Id_Intento = intentoEnCurso.Id_Intento,
                                Id_Pregunta = pregunta.Id_Pregunta
                            });
                        }
                    }
                }
                await _db.SaveChangesAsync();
            }

            var preguntasDto = preguntasMostrar.Select(p => new PreguntaDto(
                p.Id_Pregunta, p.TextoPregunta, p.ExplicacionEducativa,
                p.Opciones.Select(o => new OpcionDto(o.Id_Opcion, o.TextoOpcion)).ToList()
            )).ToList();

            return Ok(new { idIntento = intentoEnCurso.Id_Intento, preguntas = preguntasDto });
        }

        [HttpPost("cursos/{idIntento}/completar")]
        public async Task<IActionResult> CompletarCurso(int idIntento, [FromBody] ValidarRespuestasRequest request)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (request.IdsOpcionesSeleccionadas == null || !request.IdsOpcionesSeleccionadas.Any())
                return BadRequest(new { mensaje = "Debe responder todas las preguntas." });

            var intento = await _db.IntentoCursoVoluntario
                .Include(i => i.Usuario)
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(i => i.Id_Intento == idIntento && i.Id_Usuario == idUsuario);

            if (intento == null)
                return NotFound(new { mensaje = "Intento no encontrado." });

            if (intento.FechaCompletado != null)
                return BadRequest(new { mensaje = "Este intento ya fue completado o evaluado." });

            var opcionesCorrectasEsperadas = await _db.PreguntaAsginadasCurso
                .Where(pa => pa.Id_Intento == idIntento)
                .SelectMany(pa => pa.Pregunta.Opciones)
                .Where(o => o.EsCorrecta)
                .Select(o => o.Id_Opcion)
                .ToListAsync();

            bool esCorrecto = request.IdsOpcionesSeleccionadas.Count == opcionesCorrectasEsperadas.Count &&
                              request.IdsOpcionesSeleccionadas.All(id => opcionesCorrectasEsperadas.Contains(id));

            intento.FechaCompletado = DateTime.UtcNow;

            if (!esCorrecto)
            {
                intento.Exitoso = false;
                intento.PuntosObtenidos = 0;
                await _db.SaveChangesAsync();

                return BadRequest(new
                {
                    mensaje = "Algunas respuestas son incorrectas. Revisa la explicación.",
                    sugerencia = "Puedes iniciar un nuevo intento para recibir preguntas diferentes."
                });
            }

            intento.Exitoso = true;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                int diasCooldown = 30;

                var ultimoExito = await _db.IntentoCursoVoluntario
                    .Where(i => i.Id_Usuario == idUsuario && i.Id_Categoria == intento.Id_Categoria && i.Exitoso == true && i.Id_Intento != idIntento)
                    .OrderByDescending(i => i.FechaCompletado)
                    .Select(i => i.FechaCompletado)
                    .FirstOrDefaultAsync();

                bool otorgaPuntos = ultimoExito == null || (DateTime.UtcNow - ultimoExito.Value).TotalDays > diasCooldown;
                int puntosGanados = 0;
                var nombresInsigniasGanadas = new List<string>();

                if (otorgaPuntos)
                {
                    puntosGanados = 20;
                    intento.PuntosObtenidos = puntosGanados;
                    intento.Usuario.CyberScore += puntosGanados;

                    _db.Users.Update(intento.Usuario);
                    await _db.SaveChangesAsync();

                    // Lógica de Insignias
                    var insigniasNuevas = await _db.Insignias
                        .Where(i => i.PuntosRequeridos <= intento.Usuario.CyberScore &&
                                    !_db.LogrosUsuario.Any(lu => lu.Id_Usuario == idUsuario && lu.Id_Insignia == i.Id_Insignia))
                        .ToListAsync();

                    if (insigniasNuevas.Any())
                    {
                        foreach (var insignia in insigniasNuevas)
                        {
                            _db.LogrosUsuario.Add(new LogroUsuario
                            {
                                Id_Usuario = idUsuario,
                                Id_Insignia = insignia.Id_Insignia,
                                FechaObtencion = DateTime.UtcNow
                            });
                            nombresInsigniasGanadas.Add(insignia.Nombre);
                        }
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    intento.PuntosObtenidos = 0;
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = otorgaPuntos
                        ? "¡Excelente! Has dominado este tema y ganado puntos de inmunidad."
                        : "¡Buen repaso! Mantenemos tus conocimientos frescos (Sin nuevos puntos debido al enfriamiento).",
                    puntosGanados = puntosGanados,
                    insigniasDesbloqueadas = nombresInsigniasGanadas
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al procesar la gamificación del curso voluntario." });
            }
        }
    }
}
