using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace proyect.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias_pregunta",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_pregunta", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "ciudades",
                columns: table => new
                {
                    id_ciudad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciudades", x => x.id_ciudad);
                });

            migrationBuilder.CreateTable(
                name: "estados_flujo",
                columns: table => new
                {
                    id_estado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_estado = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: false),
                    categoria = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_flujo", x => x.id_estado);
                });

            migrationBuilder.CreateTable(
                name: "insignias",
                columns: table => new
                {
                    id_insignia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    icono_svg = table.Column<string>(type: "text", unicode: false, nullable: false),
                    puntos_requeridos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insignias", x => x.id_insignia);
                });

            migrationBuilder.CreateTable(
                name: "niveles_criticidad",
                columns: table => new
                {
                    id_criticidad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nivel = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_niveles_criticidad", x => x.id_criticidad);
                });

            migrationBuilder.CreateTable(
                name: "prioridades",
                columns: table => new
                {
                    id_prioridad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prioridades", x => x.id_prioridad);
                });

            migrationBuilder.CreateTable(
                name: "registros_auditoria",
                columns: table => new
                {
                    id_auditoria = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    nombre_usuario = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    accion = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    entidad = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    id_entidad_afectada = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    detalles = table.Column<string>(type: "text", nullable: true),
                    ip_origen = table.Column<string>(type: "character varying(45)", unicode: false, maxLength: 45, nullable: true),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registros_auditoria", x => x.id_auditoria);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "preguntas_concientizacion",
                columns: table => new
                {
                    id_pregunta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    texto_pregunta = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    explicacion_educativa = table.Column<string>(type: "text", nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preguntas_concientizacion", x => x.id_pregunta);
                    table.ForeignKey(
                        name: "FK_preguntas_concientizacion_categorias_pregunta_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias_pregunta",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "areas_operativas",
                columns: table => new
                {
                    id_area = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    id_criticidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_areas_operativas", x => x.id_area);
                    table.ForeignKey(
                        name: "FK_areas_operativas_niveles_criticidad_id_criticidad",
                        column: x => x.id_criticidad,
                        principalTable: "niveles_criticidad",
                        principalColumn: "id_criticidad",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plantillas_phishing",
                columns: table => new
                {
                    id_plantilla = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    asunto_correo = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    cuerpo_html = table.Column<string>(type: "text", nullable: false),
                    id_criticidad = table.Column<int>(type: "integer", nullable: false),
                    id_categoria_pregunta = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plantillas_phishing", x => x.id_plantilla);
                    table.ForeignKey(
                        name: "FK_plantillas_phishing_categorias_pregunta_id_categoria_pregun~",
                        column: x => x.id_categoria_pregunta,
                        principalTable: "categorias_pregunta",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plantillas_phishing_niveles_criticidad_id_criticidad",
                        column: x => x.id_criticidad,
                        principalTable: "niveles_criticidad",
                        principalColumn: "id_criticidad",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "opciones_respuesta",
                columns: table => new
                {
                    id_opcion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    texto_opcion = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    es_correcta = table.Column<bool>(type: "boolean", nullable: false),
                    id_pregunta = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opciones_respuesta", x => x.id_opcion);
                    table.ForeignKey(
                        name: "FK_opciones_respuesta_preguntas_concientizacion_id_pregunta",
                        column: x => x.id_pregunta,
                        principalTable: "preguntas_concientizacion",
                        principalColumn: "id_pregunta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "campanias_simulacion",
                columns: table => new
                {
                    id_campania = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    id_plantilla = table.Column<int>(type: "integer", nullable: false),
                    id_estado = table.Column<int>(type: "integer", nullable: false),
                    creado_por = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio_programada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_fin_programada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nivel_dificultad = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campanias_simulacion", x => x.id_campania);
                    table.ForeignKey(
                        name: "FK_campanias_simulacion_estados_flujo_id_estado",
                        column: x => x.id_estado,
                        principalTable: "estados_flujo",
                        principalColumn: "id_estado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_campanias_simulacion_plantillas_phishing_id_plantilla",
                        column: x => x.id_plantilla,
                        principalTable: "plantillas_phishing",
                        principalColumn: "id_plantilla",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "segmentacion_campania_area",
                columns: table => new
                {
                    id_segmentacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_campania = table.Column<int>(type: "integer", nullable: false),
                    id_area = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segmentacion_campania_area", x => x.id_segmentacion);
                    table.ForeignKey(
                        name: "FK_segmentacion_campania_area_areas_operativas_id_area",
                        column: x => x.id_area,
                        principalTable: "areas_operativas",
                        principalColumn: "id_area",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_segmentacion_campania_area_campanias_simulacion_id_campania",
                        column: x => x.id_campania,
                        principalTable: "campanias_simulacion",
                        principalColumn: "id_campania",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "envios_simulacion",
                columns: table => new
                {
                    id_envio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_campania = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    token_uuid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    fue_abierto = table.Column<bool>(type: "boolean", nullable: false),
                    hizo_clic = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_vulneracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    capacitacion_completa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_capacitacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    puntos_recuperados = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_envios_simulacion", x => x.id_envio);
                    table.ForeignKey(
                        name: "FK_envios_simulacion_campanias_simulacion_id_campania",
                        column: x => x.id_campania,
                        principalTable: "campanias_simulacion",
                        principalColumn: "id_campania",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preguntas_asignadas_envio",
                columns: table => new
                {
                    id_asignacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_envio = table.Column<int>(type: "integer", nullable: false),
                    id_pregunta = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preguntas_asignadas_envio", x => x.id_asignacion);
                    table.ForeignKey(
                        name: "FK_preguntas_asignadas_envio_envios_simulacion_id_envio",
                        column: x => x.id_envio,
                        principalTable: "envios_simulacion",
                        principalColumn: "id_envio",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_preguntas_asignadas_envio_preguntas_concientizacion_id_preg~",
                        column: x => x.id_pregunta,
                        principalTable: "preguntas_concientizacion",
                        principalColumn: "id_pregunta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "intentos_curso_voluntario",
                columns: table => new
                {
                    id_intento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_completado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    exitoso = table.Column<bool>(type: "boolean", nullable: true),
                    puntos_obtenidos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intentos_curso_voluntario", x => x.id_intento);
                    table.ForeignKey(
                        name: "FK_intentos_curso_voluntario_categorias_pregunta_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias_pregunta",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preguntas_asignadas_curso",
                columns: table => new
                {
                    id_asignacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_intento = table.Column<int>(type: "integer", nullable: false),
                    id_pregunta = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preguntas_asignadas_curso", x => x.id_asignacion);
                    table.ForeignKey(
                        name: "FK_preguntas_asignadas_curso_intentos_curso_voluntario_id_inte~",
                        column: x => x.id_intento,
                        principalTable: "intentos_curso_voluntario",
                        principalColumn: "id_intento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_preguntas_asignadas_curso_preguntas_concientizacion_id_preg~",
                        column: x => x.id_pregunta,
                        principalTable: "preguntas_concientizacion",
                        principalColumn: "id_pregunta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "logros_usuario",
                columns: table => new
                {
                    id_logro = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_insignia = table.Column<int>(type: "integer", nullable: false),
                    fecha_obtencion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logros_usuario", x => x.id_logro);
                    table.ForeignKey(
                        name: "FK_logros_usuario_insignias_id_insignia",
                        column: x => x.id_insignia,
                        principalTable: "insignias",
                        principalColumn: "id_insignia",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id_refresh_token = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", unicode: false, maxLength: 128, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_expiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_revocacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reemplazado_por_token_hash = table.Column<string>(type: "character varying(128)", unicode: false, maxLength: 128, nullable: true),
                    ip_creacion = table.Column<string>(type: "character varying(45)", unicode: false, maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id_refresh_token);
                });

            migrationBuilder.CreateTable(
                name: "rol_usuario",
                columns: table => new
                {
                    id_rol_usuario = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    asignado_por = table.Column<int>(type: "integer", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_usuario", x => x.id_rol_usuario);
                    table.ForeignKey(
                        name: "FK_rol_usuario_roles_id_rol",
                        column: x => x.id_rol,
                        principalTable: "roles",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sucursales",
                columns: table => new
                {
                    id_sucursal = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    id_ciudad = table.Column<int>(type: "integer", nullable: false),
                    direccion = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    id_gerente_responsable = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sucursales", x => x.id_sucursal);
                    table.ForeignKey(
                        name: "FK_sucursales_ciudades_id_ciudad",
                        column: x => x.id_ciudad,
                        principalTable: "ciudades",
                        principalColumn: "id_ciudad",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    apellido = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    debe_cambiar_password = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_sucursal = table.Column<int>(type: "integer", nullable: true),
                    id_area = table.Column<int>(type: "integer", nullable: true),
                    score_riesgo = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0.00m),
                    cyber_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    nombre_usuario = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: false),
                    nombre_usuario_normalizado = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    email_normalizado = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    email_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    password = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    security_stamp = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: true),
                    telefono = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: true),
                    telefono_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    factor_doble_habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    bloqueo_fin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    bloqueo_habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    intentos_fallidos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuarios_areas_operativas_id_area",
                        column: x => x.id_area,
                        principalTable: "areas_operativas",
                        principalColumn: "id_area",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuarios_sucursales_id_sucursal",
                        column: x => x.id_sucursal,
                        principalTable: "sucursales",
                        principalColumn: "id_sucursal",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_claims",
                columns: table => new
                {
                    id_usuario_claim = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    tipo_claim = table.Column<string>(type: "text", nullable: true),
                    valor_claim = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_claims", x => x.id_usuario_claim);
                    table.ForeignKey(
                        name: "FK_usuario_claims_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_logins",
                columns: table => new
                {
                    proveedor_login = table.Column<string>(type: "character varying(128)", unicode: false, maxLength: 128, nullable: false),
                    proveedor_key = table.Column<string>(type: "character varying(128)", unicode: false, maxLength: 128, nullable: false),
                    nombre_proveedor_mostrado = table.Column<string>(type: "text", nullable: true),
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_logins", x => new { x.proveedor_login, x.proveedor_key });
                    table.ForeignKey(
                        name: "FK_usuario_logins_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_tokens",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    proveedor = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    valor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_tokens", x => new { x.id_usuario, x.proveedor, x.nombre });
                    table.ForeignKey(
                        name: "FK_usuario_tokens_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categorias_pregunta",
                columns: new[] { "id_categoria", "nombre" },
                values: new object[,]
                {
                    { 1, "Falsa Urgencia y Coacción" },
                    { 2, "Suplantación de Identidad" }
                });

            migrationBuilder.InsertData(
                table: "insignias",
                columns: new[] { "id_insignia", "descripcion", "icono_svg", "nombre", "puntos_requeridos" },
                values: new object[,]
                {
                    { 1, "Completaste tu primera capacitación.", "fa-shield-halved", "Escudo de Bronce", 50 },
                    { 2, "Gran capacidad para detectar banderas rojas.", "fa-eye", "Ojo de Águila", 150 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_areas_operativas_id_criticidad",
                table: "areas_operativas",
                column: "id_criticidad");

            migrationBuilder.CreateIndex(
                name: "IX_campanias_simulacion_creado_por",
                table: "campanias_simulacion",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_campanias_simulacion_id_estado",
                table: "campanias_simulacion",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_campanias_simulacion_id_plantilla",
                table: "campanias_simulacion",
                column: "id_plantilla");

            migrationBuilder.CreateIndex(
                name: "IX_envios_simulacion_id_usuario",
                table: "envios_simulacion",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UQ_envios_token",
                table: "envios_simulacion",
                column: "token_uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EnvioSimulacion_Campania_Usuario",
                table: "envios_simulacion",
                columns: new[] { "id_campania", "id_usuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_intentos_curso_voluntario_id_categoria",
                table: "intentos_curso_voluntario",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_intentos_curso_voluntario_id_usuario",
                table: "intentos_curso_voluntario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_logros_usuario_id_insignia",
                table: "logros_usuario",
                column: "id_insignia");

            migrationBuilder.CreateIndex(
                name: "IX_logros_usuario_id_usuario",
                table: "logros_usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UQ_LogroUsuario_Usuario_Insignia",
                table: "logros_usuario",
                columns: new[] { "id_logro", "id_insignia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_opciones_respuesta_id_pregunta",
                table: "opciones_respuesta",
                column: "id_pregunta");

            migrationBuilder.CreateIndex(
                name: "IX_plantillas_phishing_id_categoria_pregunta",
                table: "plantillas_phishing",
                column: "id_categoria_pregunta");

            migrationBuilder.CreateIndex(
                name: "IX_plantillas_phishing_id_criticidad",
                table: "plantillas_phishing",
                column: "id_criticidad");

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_asignadas_curso_id_intento",
                table: "preguntas_asignadas_curso",
                column: "id_intento");

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_asignadas_curso_id_pregunta",
                table: "preguntas_asignadas_curso",
                column: "id_pregunta");

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_asignadas_envio_id_envio",
                table: "preguntas_asignadas_envio",
                column: "id_envio");

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_asignadas_envio_id_pregunta",
                table: "preguntas_asignadas_envio",
                column: "id_pregunta");

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_concientizacion_id_categoria",
                table: "preguntas_concientizacion",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_id_usuario",
                table: "refresh_tokens",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registros_auditoria_fecha_hora",
                table: "registros_auditoria",
                column: "fecha_hora");

            migrationBuilder.CreateIndex(
                name: "IX_registros_auditoria_id_usuario",
                table: "registros_auditoria",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_rol_usuario_asignado_por",
                table: "rol_usuario",
                column: "asignado_por");

            migrationBuilder.CreateIndex(
                name: "IX_rol_usuario_id_rol",
                table: "rol_usuario",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "IX_rol_usuario_id_usuario",
                table: "rol_usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_segmentacion_campania_area_id_area",
                table: "segmentacion_campania_area",
                column: "id_area");

            migrationBuilder.CreateIndex(
                name: "IX_segmentacion_campania_area_id_campania",
                table: "segmentacion_campania_area",
                column: "id_campania");

            migrationBuilder.CreateIndex(
                name: "IX_sucursales_id_ciudad",
                table: "sucursales",
                column: "id_ciudad");

            migrationBuilder.CreateIndex(
                name: "IX_sucursales_id_gerente_responsable",
                table: "sucursales",
                column: "id_gerente_responsable");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_claims_id_usuario",
                table: "usuario_claims",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_logins_id_usuario",
                table: "usuario_logins",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_area",
                table: "usuarios",
                column: "id_area");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_sucursal",
                table: "usuarios",
                column: "id_sucursal");

            migrationBuilder.CreateIndex(
                name: "UQ_usuarios_email_normalizado",
                table: "usuarios",
                column: "email_normalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_usuarios_nombre_usuario_normalizado",
                table: "usuarios",
                column: "nombre_usuario_normalizado",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_campanias_simulacion_usuarios_creado_por",
                table: "campanias_simulacion",
                column: "creado_por",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_envios_simulacion_usuarios_id_usuario",
                table: "envios_simulacion",
                column: "id_usuario",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_intentos_curso_voluntario_usuarios_id_usuario",
                table: "intentos_curso_voluntario",
                column: "id_usuario",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_logros_usuario_usuarios_id_usuario",
                table: "logros_usuario",
                column: "id_usuario",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_usuarios_id_usuario",
                table: "refresh_tokens",
                column: "id_usuario",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rol_usuario_usuarios_asignado_por",
                table: "rol_usuario",
                column: "asignado_por",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_rol_usuario_usuarios_id_usuario",
                table: "rol_usuario",
                column: "id_usuario",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sucursales_usuarios_id_gerente_responsable",
                table: "sucursales",
                column: "id_gerente_responsable",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_areas_operativas_niveles_criticidad_id_criticidad",
                table: "areas_operativas");

            migrationBuilder.DropForeignKey(
                name: "FK_sucursales_usuarios_id_gerente_responsable",
                table: "sucursales");

            migrationBuilder.DropTable(
                name: "logros_usuario");

            migrationBuilder.DropTable(
                name: "opciones_respuesta");

            migrationBuilder.DropTable(
                name: "preguntas_asignadas_curso");

            migrationBuilder.DropTable(
                name: "preguntas_asignadas_envio");

            migrationBuilder.DropTable(
                name: "prioridades");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "registros_auditoria");

            migrationBuilder.DropTable(
                name: "rol_usuario");

            migrationBuilder.DropTable(
                name: "segmentacion_campania_area");

            migrationBuilder.DropTable(
                name: "usuario_claims");

            migrationBuilder.DropTable(
                name: "usuario_logins");

            migrationBuilder.DropTable(
                name: "usuario_tokens");

            migrationBuilder.DropTable(
                name: "insignias");

            migrationBuilder.DropTable(
                name: "intentos_curso_voluntario");

            migrationBuilder.DropTable(
                name: "envios_simulacion");

            migrationBuilder.DropTable(
                name: "preguntas_concientizacion");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "campanias_simulacion");

            migrationBuilder.DropTable(
                name: "estados_flujo");

            migrationBuilder.DropTable(
                name: "plantillas_phishing");

            migrationBuilder.DropTable(
                name: "categorias_pregunta");

            migrationBuilder.DropTable(
                name: "niveles_criticidad");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "areas_operativas");

            migrationBuilder.DropTable(
                name: "sucursales");

            migrationBuilder.DropTable(
                name: "ciudades");
        }
    }
}
