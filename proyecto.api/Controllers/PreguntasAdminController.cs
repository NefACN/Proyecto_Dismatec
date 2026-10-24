using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyect.application.Abstractions;
using proyect.domain.Entidades;
using proyect.application.Dtos;
using proyect.domain.Constants;
using proyect.application;

namespace proyecto.api.Controllers
{
    [ApiController]
    [Route("api/preguntas-admin")]
    [Authorize(Roles = RolesSistemas.RolAdmin)]
    public class PreguntasAdminController : ControllerBase
    {
        private readonly IApplicationDbContext _db;
        public PreguntasAdminController(IApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public async Task<IActionResult> ListarPreguntas()
        {
            var preguntas = await _db.PregutasConcientizacion
                .Include(p => p.Categoria)
                .Include(p => p.Opciones)
                .AsNoTracking()
                .Select(p => new
                {
                    idPregunta = p.Id_Pregunta,
                    categoria = p.Categoria.Nombre,
                    idCategoria = p.Id_Categoria,
                    textoPregunta = p.TextoPregunta,
                    explicacionEducativa = p.ExplicacionEducativa,
                    opciones = p.Opciones.Select(o => new
                    {
                        idOpcion = o.Id_Opcion,
                        textoOpcion = o.TextoOpcion,
                        esCorrecta = o.EsCorrecta
                    }).ToList()
                }).ToListAsync();
            return Ok(preguntas);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPregunta(int id)
        {
            var pregunta = await _db.PregutasConcientizacion
                .Include(p => p.Opciones)
                .AsNoTracking()
                .Where(p => p.Id_Pregunta == id)
                .Select(p => new
                {
                    idPregunta = p.Id_Pregunta,
                    idCategoria = p.Id_Categoria,
                    textoPregunta = p.TextoPregunta,
                    explicacionEducativa = p.ExplicacionEducativa,
                    opciones = p.Opciones.Select(o => new
                    {
                        idOpcion = o.Id_Opcion,
                        textoOpcion = o.TextoOpcion,
                        esCorrecta = o.EsCorrecta
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (pregunta == null) return NotFound(new { mensaje = "Pregunta no encontrada." });

            return Ok(pregunta);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPregunta([FromBody] CrearPreguntaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TextoPregunta))
            {
                return BadRequest(new { mensaje = "El texto de la pregunta es obligatoio" });
            }
            if (request.Opciones == null || request.Opciones.Count < 2)
            {
                return BadRequest(new { mensaje = "Proporcione al menos 2 opciones de respuesta" });
            }
            if (!request.Opciones.Any(o => o.EsCorrecta))
            {
                return BadRequest(new { mensaje = "Debe existir al menos una opcion correcta" });
            }
            var categoExistente = await _db.CategoriasPregunta.AnyAsync(c => c.IdCategoria == request.IdCategoria);
            if (!categoExistente)
            {
                return BadRequest(new { mensaje = "La categoria no existe" });
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var nuevaPregunta = new PreguntaConcientizacion
                {
                    TextoPregunta = request.TextoPregunta,
                    ExplicacionEducativa = request.ExplicacionEducativa,
                    Id_Categoria = request.IdCategoria,
                };

                _db.PregutasConcientizacion.Add(nuevaPregunta);
                await _db.SaveChangesAsync();

                var opciones = request.Opciones.Select(o => new OpcionRespuesta
                {
                    Id_Pregunta = nuevaPregunta.Id_Pregunta,
                    TextoOpcion = o.TextoOpcion,
                    EsCorrecta = o.EsCorrecta,
                }).ToList();

                _db.OpcionesRespuesta.AddRange(opciones);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "pregunta creada exitosamente",
                    idPregunta = nuevaPregunta.Id_Pregunta
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "error al intentar crear la pregunta" });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarPregunta(int id, [FromBody] EditarPreguntasRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TextoPregunta))
                return BadRequest(new { mensaje = "El texto de la pregunta es obligatorio." });

            if (request.Opciones == null || request.Opciones.Count < 2)
                return BadRequest(new { mensaje = "Debe proporcionar al menos 2 opciones de respuesta." });

            if (!request.Opciones.Any(o => o.EsCorrecta))
                return BadRequest(new { mensaje = "Debe existir al menos una opción correcta." });

            if (!await _db.CategoriasPregunta.AnyAsync(c => c.IdCategoria == request.IdCategoria))
                return BadRequest(new { mensaje = "La categoría indicada no existe." });

            var pregunta = await _db.Set<PreguntaConcientizacion>()
                .Include(p => p.Opciones)
                .FirstOrDefaultAsync(p => p.Id_Pregunta == id);

            if (pregunta == null) return NotFound(new { mensaje = "Pregunta no encontrada." });

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                pregunta.TextoPregunta = request.TextoPregunta;
                pregunta.ExplicacionEducativa = request.ExplicacionEducativa;
                pregunta.Id_Categoria = request.IdCategoria;

                var idsOpcionesRequest = request.Opciones
                    .Where(o => o.IdOpcion.HasValue)
                    .Select(o => o.IdOpcion.GetValueOrDefault())
                    .ToList();

                var opcionesAEliminar = pregunta.Opciones
                    .Where(o => !idsOpcionesRequest.Contains(o.Id_Opcion))
                    .ToList();
                _db.OpcionesRespuesta.RemoveRange(opcionesAEliminar);

                foreach (var opcRequest in request.Opciones)
                {
                    if (opcRequest.IdOpcion.HasValue)
                    {
                        var opcionExistente = pregunta.Opciones.FirstOrDefault(o => o.Id_Opcion == opcRequest.IdOpcion.Value);
                        if (opcionExistente != null)
                        {
                            opcionExistente.TextoOpcion = opcRequest.TextoOpcion;
                            opcionExistente.EsCorrecta = opcRequest.EsCorrecta;
                        }
                    }
                    else
                    {
                        pregunta.Opciones.Add(new OpcionRespuesta
                        {
                            TextoOpcion = opcRequest.TextoOpcion,
                            EsCorrecta = opcRequest.EsCorrecta
                        });
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error interno al editar la pregunta." });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarPregunta(int id)
        {
            var pregunta = await _db.Set<PreguntaConcientizacion>().FindAsync(id);
            if (pregunta == null) return NotFound(new { mensaje = "Pregunta no encontrada." });

            
            bool usadaEnPhishing = await _db.PreguntasAsignadasEnvio.AnyAsync(pa => pa.Id_Pregunta == id);
            bool usadaEnCurso = await _db.PreguntaAsginadasCurso.AnyAsync(pa => pa.Id_Pregunta == id);

            if (usadaEnPhishing || usadaEnCurso)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar la pregunta porque ya ha sido respondida por usuarios en campañas o cursos voluntarios. Borrarla afectaría el historial de auditoría y puntajes."
                });
            }

            try
            {
                _db.Set<PreguntaConcientizacion>().Remove(pregunta);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "Error al intentar eliminar la pregunta." });
            }
        }
    }
}
