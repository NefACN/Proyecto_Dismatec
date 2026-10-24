using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using proyect.domain.Entidades;

namespace proyect.application.Abstractions;

// EF query boundary; implementations and provider configuration belong to Infrastructure.
public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Rol> Roles { get; }
    DbSet<RolUsuario> RolUsuario { get; }
    DbSet<RegistroAuditoria> RegistrosAuditoria { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Ciudad> Ciudades { get; }
    DbSet<NivelCriticidad> NivelesCriticidad { get; }
    DbSet<EstadoFlujo> EstadosFlujo { get; }
    DbSet<Prioridad> Prioridades { get; }
    DbSet<AreaOperativa> AreasOperativas { get; }
    DbSet<Sucursal> Sucursales { get; }
    DbSet<PlantillaPhishing> PlantillasPhishing { get; }
    DbSet<CampaniaSimulacion> CampaniasSimulacion { get; }
    DbSet<SegmentacionCampaniArea> SegmentacionesCampaniaArea { get; }
    DbSet<EnvioSimulacion> EnviosSimulacion { get; }
    DbSet<CategoriaPregunta> CategoriasPregunta { get; }
    DbSet<PreguntaConcientizacion> PregutasConcientizacion { get; }
    DbSet<OpcionRespuesta> OpcionesRespuesta { get; }
    DbSet<Insignia> Insignias { get; }
    DbSet<LogroUsuario> LogrosUsuario { get; }
    DbSet<PreguntaAsignadaEnvio> PreguntasAsignadasEnvio { get; }
    DbSet<IntentoCursoVoluntario> IntentoCursoVoluntario { get; }
    DbSet<PreguntaAsignadaCurso> PreguntaAsginadasCurso { get; }
    DatabaseFacade Database { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
