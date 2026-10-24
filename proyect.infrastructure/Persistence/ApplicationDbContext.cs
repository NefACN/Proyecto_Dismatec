using proyect.application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using proyect.domain.Entidades;

namespace proyect.infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityUserContext<ApplicationUser, int>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<RolUsuario> RolUsuario => Set<RolUsuario>();

        public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // de la gestion
        public DbSet<Ciudad> Ciudades => Set<Ciudad>();
        public DbSet<NivelCriticidad> NivelesCriticidad => Set<NivelCriticidad>();
        public DbSet<EstadoFlujo> EstadosFlujo => Set<EstadoFlujo>();
        public DbSet<Prioridad> Prioridades => Set<Prioridad>();

        public DbSet<AreaOperativa> AreasOperativas => Set<AreaOperativa>();
        public DbSet<Sucursal> Sucursales => Set<Sucursal>();
        public DbSet<PlantillaPhishing> PlantillasPhishing => Set<PlantillaPhishing>();
        public DbSet<CampaniaSimulacion> CampaniasSimulacion => Set<CampaniaSimulacion>();
        public DbSet<SegmentacionCampaniArea> SegmentacionesCampaniaArea => Set<SegmentacionCampaniArea>();
        public DbSet<EnvioSimulacion> EnviosSimulacion => Set<EnvioSimulacion>();

        //concientizacion
        public DbSet<CategoriaPregunta> CategoriasPregunta => Set<CategoriaPregunta>();
        public DbSet<PreguntaConcientizacion> PregutasConcientizacion => Set<PreguntaConcientizacion>();
        public DbSet<OpcionRespuesta> OpcionesRespuesta => Set<OpcionRespuesta>();
        public DbSet<Insignia> Insignias => Set<Insignia>();
        public DbSet<LogroUsuario> LogrosUsuario => Set<LogroUsuario>();

        public DbSet<PreguntaAsignadaEnvio> PreguntasAsignadasEnvio => Set<PreguntaAsignadaEnvio>();

        public DbSet<IntentoCursoVoluntario> IntentoCursoVoluntario => Set < IntentoCursoVoluntario>();
        public DbSet<PreguntaAsignadaCurso> PreguntaAsginadasCurso => Set<PreguntaAsignadaCurso>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("usuarios");
                entity.Property(u => u.Id).HasColumnName("id_usuario");
                entity.Property(u => u.UserName).HasColumnName("nombre_usuario").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(u => u.NormalizedUserName).HasColumnName("nombre_usuario_normalizado").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(u => u.PasswordHash).HasColumnName("password").HasMaxLength(255).IsUnicode(false).IsRequired();
                entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(u => u.NormalizedEmail).HasColumnName("email_normalizado").HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(u => u.EmailConfirmed).HasColumnName("email_confirmado").IsRequired();
                entity.Property(u => u.PhoneNumber).HasColumnName("telefono").HasMaxLength(20).IsUnicode(false);
                entity.Property(u => u.PhoneNumberConfirmed).HasColumnName("telefono_confirmado").IsRequired();
                entity.Property(u => u.TwoFactorEnabled).HasColumnName("factor_doble_habilitado").IsRequired();
                entity.Property(u => u.LockoutEnabled).HasColumnName("bloqueo_habilitado").IsRequired();
                entity.Property(u => u.LockoutEnd).HasColumnName("bloqueo_fin");
                entity.Property(u => u.AccessFailedCount).HasColumnName("intentos_fallidos").IsRequired();
                entity.Property(u => u.SecurityStamp).HasColumnName("security_stamp").HasMaxLength(255).IsUnicode(false);
                entity.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp").HasMaxLength(255).IsUnicode(false);

                entity.Property(u => u.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(u => u.Apellido).HasColumnName("apellido").HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(u => u.Activo).HasColumnName("activo").IsRequired();
                entity.Property(u => u.DebeCambiarPassword).HasColumnName("debe_cambiar_password");
                entity.Property(u => u.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("timestamp with time zone").IsRequired();
                entity.Property(u => u.FechaModificacion).HasColumnName("fecha_modificacion").HasColumnType("timestamp with time zone");

                entity.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UQ_usuarios_nombre_usuario_normalizado").IsUnique();
                entity.HasIndex(u => u.NormalizedEmail).HasDatabaseName("UQ_usuarios_email_normalizado").IsUnique();

                // gestion sucursal, arera 
                entity.Property(u => u.Id_Sucursal).HasColumnName("id_sucursal");
                entity.Property(u => u.Id_Area).HasColumnName("id_area");

                entity.Property(u => u.ScoreRiesgo).HasColumnName("score_riesgo").HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(0.00m).IsRequired();
                entity.HasOne(u => u.Sucursal)
                      .WithMany()
                      .HasForeignKey(u => u.Id_Sucursal)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(u => u.AreaOperativa)
                      .WithMany()
                      .HasForeignKey(u => u.Id_Area)
                      .OnDelete(DeleteBehavior.Restrict);

                //concientizacion
                entity.Property(u => u.CyberScore).HasColumnName("cyber_score").IsRequired().HasDefaultValue(0);

            });

            
            builder.Entity<IdentityUserToken<int>>(entity =>
            {
                entity.ToTable("usuario_tokens");
                entity.Property(t => t.UserId).HasColumnName("id_usuario");
                entity.Property(t => t.LoginProvider).HasColumnName("proveedor").HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(t => t.Name).HasColumnName("nombre").HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(t => t.Value).HasColumnName("valor");
            });

           
            builder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.ToTable("usuario_claims");
                entity.Property(c => c.Id).HasColumnName("id_usuario_claim");
                entity.Property(c => c.UserId).HasColumnName("id_usuario");
                entity.Property(c => c.ClaimType).HasColumnName("tipo_claim");
                entity.Property(c => c.ClaimValue).HasColumnName("valor_claim");
            });

            
            builder.Entity<IdentityUserLogin<int>>(entity =>
            {
                entity.ToTable("usuario_logins");
                entity.Property(l => l.LoginProvider).HasColumnName("proveedor_login").HasMaxLength(128).IsUnicode(false).IsRequired();
                entity.Property(l => l.ProviderKey).HasColumnName("proveedor_key").HasMaxLength(128).IsUnicode(false).IsRequired();
                entity.Property(l => l.ProviderDisplayName).HasColumnName("nombre_proveedor_mostrado");
                entity.Property(l => l.UserId).HasColumnName("id_usuario").IsRequired();
            });

            
            builder.Entity<Rol>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(r => r.IdRol);
                entity.Property(r => r.IdRol).HasColumnName("id_rol");
                entity.Property(r => r.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(r => r.Descripcion).HasColumnName("descripcion").HasMaxLength(255).IsUnicode(false);
                entity.Property(r => r.Activo).HasColumnName("activo").IsRequired();
                entity.Property(r => r.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("timestamp with time zone").IsRequired();
                entity.Property(r => r.FechaModificacion).HasColumnName("fecha_modificacion").HasColumnType("timestamp with time zone");
            });

            
            builder.Entity<RolUsuario>(entity =>
            {
                entity.ToTable("rol_usuario");
                entity.HasKey(ru => ru.IdRolUsuario);
                entity.Property(ru => ru.IdRolUsuario).HasColumnName("id_rol_usuario");
                entity.Property(ru => ru.IdUsuario).HasColumnName("id_usuario").IsRequired();
                entity.Property(ru => ru.IdRol).HasColumnName("id_rol").IsRequired();
                entity.Property(ru => ru.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("timestamp with time zone").IsRequired();
                entity.Property(ru => ru.FechaFin).HasColumnName("fecha_fin").HasColumnType("timestamp with time zone");
                entity.Property(ru => ru.Activo).HasColumnName("activo").IsRequired();
                entity.Property(ru => ru.AsignadoPor).HasColumnName("asignado_por");
                entity.Property(ru => ru.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("timestamp with time zone").IsRequired();
                entity.Property(ru => ru.FechaModificacion).HasColumnName("fecha_modificacion").HasColumnType("timestamp with time zone");

                entity.HasOne(ru => ru.Usuario)
                      .WithMany(u => u.RolesAsignados)
                      .HasForeignKey(ru => ru.IdUsuario)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ru => ru.Rol)
                      .WithMany(r => r.UsuariosAsignados)
                      .HasForeignKey(ru => ru.IdRol)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ru => ru.AsignadoPorUsuario)
                      .WithMany()
                      .HasForeignKey(ru => ru.AsignadoPor)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //de la auditoria
            builder.Entity<RegistroAuditoria>(entity =>
            {
                entity.ToTable("registros_auditoria");
                entity.HasKey(e => e.Id_Auditoria);
                entity.Property(e => e.Id_Auditoria).HasColumnName("id_auditoria");
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.NombreUsuario).HasColumnName("nombre_usuario").HasMaxLength(30).IsUnicode(false);
                entity.Property(e => e.Accion).HasColumnName("accion").HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(e => e.Entidad).HasColumnName("entidad").HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.IdEntidadAfectada).HasColumnName("id_entidad_afectada").HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.Detalles).HasColumnName("detalles");
                entity.Property(e => e.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45).IsUnicode(false);
                entity.Property(e => e.Exitoso).HasColumnName("exitoso").IsRequired();
                entity.Property(e => e.FechaHora).HasColumnName("fecha_hora").HasColumnType("timestamp with time zone").IsRequired();

                entity.HasIndex(e => e.FechaHora);
                entity.HasIndex(e => e.IdUsuario);
            });
            //Tokens
            builder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.Id_RefreshToken);
                entity.Property(e => e.Id_RefreshToken).HasColumnName("id_refresh_token");
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario").IsRequired();
                entity.Property(e => e.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsUnicode(false).IsRequired();
                entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("timestamp with time zone").IsRequired();
                entity.Property(e => e.FechaExpiracion).HasColumnName("fecha_expiracion").HasColumnType("timestamp with time zone").IsRequired();
                entity.Property(e => e.FechaRevocacion).HasColumnName("fecha_revocacion").HasColumnType("timestamp with time zone");
                entity.Property(e => e.ReemplazadoPorTokenHash).HasColumnName("reemplazado_por_token_hash").HasMaxLength(128).IsUnicode(false);
                entity.Property(e => e.IpCreacion).HasColumnName("ip_creacion").HasMaxLength(45).IsUnicode(false);

                entity.HasIndex(e => e.TokenHash).IsUnique();

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.IdUsuario)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Ciudad>(entity =>
            {
                entity.ToTable("ciudades");
                entity.HasKey(c => c.Id_Ciudad);
                entity.Property(c => c.Id_Ciudad).HasColumnName("id_ciudad");
                entity.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            });

            builder.Entity<NivelCriticidad>(entity =>
            {
                entity.ToTable("niveles_criticidad");
                entity.HasKey(nc => nc.Id_NivelCriticidad);
                entity.Property(nc => nc.Id_NivelCriticidad).HasColumnName("id_criticidad");
                entity.Property(nc => nc.Nivel).HasColumnName("nivel").IsUnicode(false).IsRequired().HasMaxLength(20);
            });
            builder.Entity<Prioridad>(entity =>
            {
                entity.ToTable("prioridades");
                entity.HasKey(p => p.Id_Prioridad);
                entity.Property(p => p.Id_Prioridad).HasColumnName("id_prioridad");
                entity.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(20).IsUnicode(false).IsRequired();
            });
            builder.Entity<EstadoFlujo>(entity =>
            {
                entity.ToTable("estados_flujo");
                entity.HasKey(e => e.Id_Estado);
                entity.Property(e => e.Id_Estado).HasColumnName("id_estado");
                entity.Property(e => e.NombreEstado).HasColumnName("nombre_estado").HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(e => e.Categoria).HasColumnName("categoria").HasMaxLength(30).IsUnicode(false).IsRequired();
            });

            builder.Entity<AreaOperativa>(entity =>
            {
                entity.ToTable("areas_operativas");
                entity.HasKey(e => e.Id_Area);
                entity.Property(e => e.Id_Area).HasColumnName("id_area");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(e => e.Id_Criticidad).HasColumnName("id_criticidad").IsRequired();

                entity.HasOne(e => e.Criticidad)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Criticidad)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Sucursal>(entity =>
            {
                entity.ToTable("sucursales");
                entity.HasKey(e => e.Id_Sucursal);
                entity.Property(e => e.Id_Sucursal).HasColumnName("id_sucursal");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(e => e.Id_Ciudad).HasColumnName("id_ciudad").IsRequired();
                entity.Property(e => e.Direccion).HasColumnName("direccion").HasMaxLength(100).IsUnicode(false);
                entity.Property(e => e.Id_GerenteResponsable).HasColumnName("id_gerente_responsable");
                entity.Property(e => e.Activo).HasColumnName("activo").IsRequired();

                entity.HasOne(e => e.Ciudad)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Ciudad)
                      .OnDelete(DeleteBehavior.Restrict);

                // gerente no se borra si se borra la sucursal 
                entity.HasOne(e => e.GerenteResponsable)
                      .WithMany()
                      .HasForeignKey(e => e.Id_GerenteResponsable)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //Simulaciones
            builder.Entity<PlantillaPhishing>(entity =>
            {
                entity.ToTable("plantillas_phishing");
                entity.HasKey(e => e.Id_Plantilla);
                entity.Property(e => e.Id_Plantilla).HasColumnName("id_plantilla");

                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.AsuntoCorreo).HasColumnName("asunto_correo").HasMaxLength(150).IsUnicode(false).IsRequired();

                entity.Property(e => e.CuerpoHtml).HasColumnName("cuerpo_html").IsRequired();

                entity.Property(e => e.Id_Criticidad).HasColumnName("id_criticidad").IsRequired();

                entity.Property(e => e.Id_CategoriaPegunta).HasColumnName("id_categoria_pregunta").IsRequired().HasDefaultValue(1);

                entity.HasOne(e => e.Criticidad)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Criticidad)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CategoriaPregunta)
                .WithMany().HasForeignKey(e => e.Id_CategoriaPegunta).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CampaniaSimulacion>(entity =>
            {
                entity.ToTable("campanias_simulacion");
                entity.HasKey(e => e.Id_Campania);
                entity.Property(e => e.Id_Campania).HasColumnName("id_campania");

                entity.Property(e => e.Titulo).HasColumnName("titulo").HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(e => e.Id_Plantilla).HasColumnName("id_plantilla").IsRequired();
                entity.Property(e => e.Id_Estado).HasColumnName("id_estado").IsRequired();
                entity.Property(e => e.CreadoPor).HasColumnName("creado_por").IsRequired();
                entity.Property(e => e.FechaInicioProgramada).HasColumnName("fecha_inicio_programada").HasColumnType("timestamp with time zone");
                entity.Property(e => e.FechaFinProgramada).HasColumnName("fecha_fin_programada").HasColumnType("timestamp with time zone");

                entity.HasOne(e => e.Plantilla)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Plantilla)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Estado)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Estado)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Creador)
                      .WithMany()
                      .HasForeignKey(e => e.CreadoPor)
                      .OnDelete(DeleteBehavior.Restrict);

                //concientizacion
                entity.Property(e => e.NivelDificultad).HasColumnName("nivel_dificultad").IsRequired().HasDefaultValue(1);
            });

            builder.Entity<SegmentacionCampaniArea>(entity =>
            {
                entity.ToTable("segmentacion_campania_area");
                entity.HasKey(e => e.Id_Segmentacion);
                entity.Property(e => e.Id_Segmentacion).HasColumnName("id_segmentacion");
                entity.Property(e => e.Id_Campania).HasColumnName("id_campania").IsRequired();
                entity.Property(e => e.Id_Area).HasColumnName("id_area").IsRequired();

                entity.HasOne(e => e.Campania)
                      .WithMany(c => c.Segmentaciones)
                      .HasForeignKey(e => e.Id_Campania)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AreaOperativa)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Area)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<EnvioSimulacion>(entity =>
            {
                entity.ToTable("envios_simulacion");
                entity.HasKey(e => e.Id_Envio);
                entity.Property(e => e.Id_Envio).HasColumnName("id_envio");

                entity.Property(e => e.Id_Campania).HasColumnName("id_campania").IsRequired();
                entity.Property(e => e.Id_Usuario).HasColumnName("id_usuario").IsRequired();

                entity.Property(e => e.TokenUuid)
                      .HasColumnName("token_uuid")
                      .HasDefaultValueSql("gen_random_uuid()")
                      .ValueGeneratedOnAdd()
                      .IsRequired();

                entity.Property(e => e.FueAbierto).HasColumnName("fue_abierto").IsRequired();
                entity.Property(e => e.HizoClick).HasColumnName("hizo_clic").IsRequired();
                entity.Property(e => e.FechaVulneracion).HasColumnName("fecha_vulneracion").HasColumnType("timestamp with time zone");

                entity.HasIndex(e => e.TokenUuid).HasDatabaseName("UQ_envios_token").IsUnique();

                entity.HasOne(e => e.Campania)
                      .WithMany(c => c.Envios)
                      .HasForeignKey(e => e.Id_Campania)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Usuario)
                      .OnDelete(DeleteBehavior.Restrict);

                //concientizacion
                entity.Property(e => e.CapacitacionCompleta).HasColumnName("capacitacion_completa").IsRequired().HasDefaultValue(false);
                entity.Property(e => e.FechaCapacitacion).HasColumnName("fecha_capacitacion").HasColumnType("timestamp with time zone");
                entity.Property(e => e.PuntosRecuperados).HasColumnName("puntos_recuperados").IsRequired().HasDefaultValue(0);

            });

            //capacitacion
            builder.Entity<CategoriaPregunta>(entity =>
            {
                entity.ToTable("categorias_pregunta");
                entity.HasKey(e => e.IdCategoria);
                entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            });

            builder.Entity<PreguntaConcientizacion>(entity =>
            {
                entity.ToTable("preguntas_concientizacion");
                entity.HasKey(e => e.Id_Pregunta);
                entity.Property(e => e.Id_Pregunta).HasColumnName("id_pregunta");
                entity.Property(e => e.TextoPregunta).HasColumnName("texto_pregunta").HasMaxLength(255).IsUnicode(false).IsRequired();
                entity.Property(e => e.ExplicacionEducativa).HasColumnName("explicacion_educativa").IsRequired();
                entity.Property(e => e.Id_Categoria).HasColumnName("id_categoria").IsRequired();

                entity.HasOne(e => e.Categoria)
                      .WithMany(c => c.Preguntas)
                      .HasForeignKey(e => e.Id_Categoria)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OpcionRespuesta>(entity =>
            {
                entity.ToTable("opciones_respuesta");
                entity.HasKey(e => e.Id_Opcion);
                entity.Property(e => e.Id_Opcion).HasColumnName("id_opcion");
                entity.Property(e => e.TextoOpcion).HasColumnName("texto_opcion").HasMaxLength(255).IsUnicode(false).IsRequired();
                entity.Property(e => e.EsCorrecta).HasColumnName("es_correcta").IsRequired();
                entity.Property(e => e.Id_Pregunta).HasColumnName("id_pregunta").IsRequired();

                entity.HasOne(e => e.Pregunta)
                      .WithMany(p => p.Opciones)
                      .HasForeignKey(e => e.Id_Pregunta)
                      .OnDelete(DeleteBehavior.Cascade); // borra pregunta, borra opciones
            });

            // "Gamificacion"

            builder.Entity<Insignia>(entity =>
            {
                entity.ToTable("insignias");
                entity.HasKey(e => e.Id_Insignia);
                entity.Property(e => e.Id_Insignia).HasColumnName("id_insignia");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(150).IsUnicode(false).IsRequired();
                entity.Property(e => e.IconoSvg).HasColumnName("icono_svg").IsUnicode(false);
                entity.Property(e => e.PuntosRequeridos).HasColumnName("puntos_requeridos").IsRequired();
            });

            builder.Entity<LogroUsuario>(entity =>
            {
                entity.ToTable("logros_usuario");
                entity.HasKey(e => e.Id_Logro);
                entity.Property(e => e.Id_Logro).HasColumnName("id_logro");
                entity.Property(e => e.Id_Usuario).HasColumnName("id_usuario").IsRequired();
                entity.Property(e => e.Id_Insignia).HasColumnName("id_insignia").IsRequired();
                entity.Property(e => e.FechaObtencion).HasColumnName("fecha_obtencion").HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.LogrosObtenidos)
                      .HasForeignKey(e => e.Id_Usuario)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Insignia)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Insignia)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PreguntaAsignadaEnvio>(entity => {
                entity.ToTable("preguntas_asignadas_envio");
                entity.HasKey(e => e.Id_Asignacion);
                entity.Property(e => e.Id_Asignacion).HasColumnName("id_asignacion");
                entity.Property(e => e.Id_Envio).HasColumnName("id_envio").IsRequired();
                entity.Property(e => e.Id_Pregunta).HasColumnName("id_pregunta").IsRequired();

                entity.HasOne(e => e.Envio).WithMany().HasForeignKey(e => e.Id_Envio)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Pregunta).WithMany().HasForeignKey(e => e.Id_Pregunta)
                .OnDelete(DeleteBehavior.Restrict);
            });

            //sending
            // SEEDING DE DATOS (Agrega esto al final del OnModelCreating)
            builder.Entity<CategoriaPregunta>().HasData(
                new CategoriaPregunta { IdCategoria = 1, Nombre = "Falsa Urgencia y Coacción" },
                new CategoriaPregunta { IdCategoria = 2, Nombre = "Suplantación de Identidad" }
            );

            builder.Entity<Insignia>().HasData(
                new Insignia { Id_Insignia = 1, Nombre = "Escudo de Bronce", Descripcion = "Completaste tu primera capacitación.", PuntosRequeridos = 50, IconoSvg = "fa-shield-halved" },
                new Insignia { Id_Insignia = 2, Nombre = "Ojo de Águila", Descripcion = "Gran capacidad para detectar banderas rojas.", PuntosRequeridos = 150, IconoSvg = "fa-eye" }
            );

            //indicesss
            builder.Entity<EnvioSimulacion>()
                .HasIndex(e => new { e.Id_Campania, e.Id_Usuario })
                .HasDatabaseName("UQ_EnvioSimulacion_Campania_Usuario")
                .IsUnique();

            builder.Entity<LogroUsuario>()
                .HasIndex(l => new { l.Id_Logro, l.Id_Insignia })
                .HasDatabaseName("UQ_LogroUsuario_Usuario_Insignia")
                .IsUnique();

            //voluntario

            builder.Entity<IntentoCursoVoluntario>(entity =>
            {
                entity.ToTable("intentos_curso_voluntario");
                entity.HasKey(e => e.Id_Intento);
                entity.Property(e => e.Id_Intento).HasColumnName("id_intento");
                entity.Property(e => e.Id_Usuario).HasColumnName("id_usuario").IsRequired();
                entity.Property(e => e.Id_Categoria).HasColumnName("id_categoria").IsRequired();
                entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("timestamp with time zone").IsRequired();
                entity.Property(e => e.FechaCompletado).HasColumnName("fecha_completado").HasColumnType("timestamp with time zone");
                entity.Property(e => e.Exitoso).HasColumnName("exitoso");
                entity.Property(e => e.PuntosObtenidos).HasColumnName("puntos_obtenidos").IsRequired().HasDefaultValue(0);

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Usuario)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Categoria)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Categoria)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PreguntaAsignadaCurso>(entity =>
            {
                entity.ToTable("preguntas_asignadas_curso");
                entity.HasKey(e => e.Id_Asignacion);
                entity.Property(e => e.Id_Asignacion).HasColumnName("id_asignacion");
                entity.Property(e => e.Id_Intento).HasColumnName("id_intento").IsRequired();
                entity.Property(e => e.Id_Pregunta).HasColumnName("id_pregunta").IsRequired();

                entity.HasOne(e => e.Intento)
                      .WithMany(i => i.PreguntasAsignadas)
                      .HasForeignKey(e => e.Id_Intento)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Pregunta)
                      .WithMany()
                      .HasForeignKey(e => e.Id_Pregunta)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            foreach (var entity in builder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
            {
                if (property.GetColumnType() == "timestamp with time zone" &&
                    (Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == typeof(DateTime))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc),
                        value => DateTime.SpecifyKind(value, DateTimeKind.Utc)));
                }
            }
        }
    }
}
