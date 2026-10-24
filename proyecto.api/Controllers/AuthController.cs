using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyect.application.Abstractions;
using proyect.domain.Entidades;
using proyect.application.Dtos;
using proyect.application.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace proyecto.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IConfiguration _configuration;

        private readonly IAuditoriaService _auditoria;
        private readonly IRefreshTokenService _refreshTokenService;

        private const string MensajeSesionInvalida = "Sesión inválida o expirada, vuelve a iniciar sesión.";
        private const string MensajeVerificacionInvalida = "Sesión de verificación inválida o expirada, vuelve a iniciar sesión.";
        private const string MensajeCredencialesInvalidas = "Usuario o contraseña incorrectos.";

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IApplicationDbContext db,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IConfiguration configuration,
            IAuditoriaService auditoriaService,
            IRefreshTokenService refreshTokenService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _db = db;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _configuration = configuration;
            _auditoria = auditoriaService;
            _refreshTokenService = refreshTokenService;
        }

        //validar usuario y su password
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var usuario = await _userManager.FindByNameAsync(request.NombreUsuario);

            if (usuario is null)
            {
                await _auditoria.RegistrarAsync("LoginFallido", exitoso: false, nombreUsuario: request.NombreUsuario);
                return Unauthorized(new { mensaje = MensajeCredencialesInvalidas });
            }

            var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, request.Password, lockoutOnFailure: true);

            if (resultado.IsLockedOut)
            {
                await _auditoria.RegistrarAsync("Login Fallido", exitoso: false, idUsuario: usuario.Id, nombreUsuario: usuario.UserName, detalles: new { razon = "Bloqueado" });
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });
            }

            if (!resultado.Succeeded)
            {
                await _auditoria.RegistrarAsync("LoginFallido", exitoso: false, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);
                return Unauthorized(new { mensaje = MensajeCredencialesInvalidas });
            }

            if (!usuario.Activo)
            {
                await _auditoria.RegistrarAsync("LoginFallido", exitoso: false, idUsuario: usuario.Id, nombreUsuario: usuario.UserName, detalles: new { razon = "Inactivo" });
                return Unauthorized(new { mensaje = MensajeCredencialesInvalidas });
            }

            if (usuario.DebeCambiarPassword)
            {
                var cambioToken = _jwtTokenService.GenerarTokenCambioPasswordObligatorio(usuario);
                return Ok(new LoginResponse(
                    RequiereCambioPassword: true, CambioPasswordToken: cambioToken,
                    RequiereMfa: false, Token: null, Refreshtoken: null, MfaToken: null, MetodosDisponibles: null));
            }

            await _auditoria.RegistrarAsync("Login", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);
            return await ContinuarLoginTrasPasswordAsync(usuario);
        }

        [HttpPost("cambiar-password-obligatorio")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> CambiarPasswordObligatorio(CambiarPasswordObligatorioRequest request)
        {
            var idUsuario = _jwtTokenService.ValidarTokenCambioPasswordObligatorio(request.CambioPasswordToken);
            if (idUsuario is null)
                return Unauthorized(new { mensaje = MensajeSesionInvalida });

            var usuario = await _userManager.FindByIdAsync(idUsuario.Value.ToString());

            if (usuario is null || !usuario.Activo)
                return Unauthorized(new { mensaje = MensajeSesionInvalida });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultado = await _userManager.ResetPasswordAsync(usuario, resetToken, request.PasswordNueva);

            if (!resultado.Succeeded)
                return BadRequest(new { errores = resultado.Errors.Select(e => e.Description) });

            usuario.DebeCambiarPassword = false;
            usuario.FechaModificacion = DateTime.UtcNow;
            await _userManager.UpdateAsync(usuario);

            await _auditoria.RegistrarAsync("CambioPassword", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);
            await RevocarTokenDeUsuarioAsync(usuario.Id);

            return await ContinuarLoginTrasPasswordAsync(usuario);
        }

        private async Task<ActionResult<LoginResponse>> ContinuarLoginTrasPasswordAsync(ApplicationUser usuario)
        {
            if (await _userManager.GetTwoFactorEnabledAsync(usuario))
            {
                var metodos = await _userManager.GetValidTwoFactorProvidersAsync(usuario);
                var mfaToken = _jwtTokenService.GenerarTokenMfaPending(usuario);

                return Ok(new LoginResponse(
                    RequiereCambioPassword: false, CambioPasswordToken: null,
                    RequiereMfa: true, Token: null, Refreshtoken: null, MfaToken: mfaToken, MetodosDisponibles: metodos.ToList()));
            }
            var (refreshTokenPlano, refreshTokenEntidad) = _refreshTokenService.Generar(usuario.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
            _db.RefreshTokens.Add(refreshTokenEntidad);
            await _db.SaveChangesAsync();

            await _auditoria.RegistrarAsync("Login", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);

            var roles = await ObtenerRolesAsync(usuario.Id);
            var token = _jwtTokenService.GenerarToken(usuario, roles);
            return Ok(new LoginResponse(
                RequiereCambioPassword: false, CambioPasswordToken: null,
                RequiereMfa: false, Token: token, Refreshtoken: null, MfaToken: null, MetodosDisponibles: null));
        }

        [HttpPost("enviar-codigo-mfa")]
        [AllowAnonymous]
        public async Task<IActionResult> EnviarCodigoMfa(EnviarCodigoMfaRequest request)
        {
            var idUsuario = _jwtTokenService.ValidarTokenMfaPending(request.MfaToken);
            if (idUsuario is null)
                return Unauthorized(new { mensaje = MensajeVerificacionInvalida });

            var usuario = await _userManager.FindByIdAsync(idUsuario.Value.ToString());
            if (usuario is null || !usuario.Activo)
                return Unauthorized(new { mensaje = MensajeVerificacionInvalida });
            
            if (await _userManager.IsLockedOutAsync(usuario))
            {
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });
            }

            if (request.Metodo == TokenOptions.DefaultAuthenticatorProvider)
                return BadRequest(new { mensaje = "El método autenticador no requiere envío de código." });

            var codigo = await _userManager.GenerateTwoFactorTokenAsync(usuario, request.Metodo);

            if (request.Metodo == TokenOptions.DefaultEmailProvider)
            {
                if (string.IsNullOrWhiteSpace(usuario.Email))
                    return BadRequest(new { mensaje = "El usuario no tiene un correo registrado." });

                var cuerpoHtml = $"<p>Hola {usuario.Nombre},</p>" +
                                  $"<p>Tu código de verificación es:</p><h2>{codigo}</h2>" +
                                  "<p>Si no fuiste tú quien intentó iniciar sesión, ignora este mensaje.</p>";

                await _emailSender.EnviarAsync(usuario.Email, $"{usuario.Nombre} {usuario.Apellido}", "Tu código de verificación", cuerpoHtml);
            }
            else if (request.Metodo == TokenOptions.DefaultPhoneProvider)
            {
                if (string.IsNullOrWhiteSpace(usuario.PhoneNumber))
                    return BadRequest(new { mensaje = "El usuario no tiene un teléfono registrado." });

                await _smsSender.EnviarAsync(usuario.PhoneNumber, $"Tu código de verificación es: {codigo}");
            }
            else
            {
                return BadRequest(new { mensaje = "Método no reconocido." });
            }

            return Ok(new { mensaje = $"Código enviado por {request.Metodo}." });
        }

        [HttpPost("verify-mfa")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> VerifyMfa(VerifyMfaRequest request)
        {
            var idUsuario = _jwtTokenService.ValidarTokenMfaPending(request.MfaToken);
            if (idUsuario is null)
                return Unauthorized(new { mensaje = MensajeVerificacionInvalida });

            var usuario = await _userManager.FindByIdAsync(idUsuario.Value.ToString());
            if (usuario is null || !usuario.Activo)
                return Unauthorized(new { mensaje = MensajeVerificacionInvalida });
            
            if (await _userManager.IsLockedOutAsync(usuario))
            {
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });
            }

            var esValido = await _userManager.VerifyTwoFactorTokenAsync(usuario, request.Metodo, request.Codigo);
            if (!esValido)
            {
                await _userManager.AccessFailedAsync(usuario);
                await _auditoria.RegistrarAsync("VerificacionMfa", exitoso: false, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);
                return Unauthorized(new { mensaje = "Código incorrecto o expirado." });
            }

            await _userManager.ResetAccessFailedCountAsync(usuario);

            var (refreshTokenPlano, refreshTokenEntidad) = _refreshTokenService.Generar(usuario.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
            _db.RefreshTokens.Add(refreshTokenEntidad);
            await _db.SaveChangesAsync();

            await _auditoria.RegistrarAsync("VerificacionMfa", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);
            await _auditoria.RegistrarAsync("Login", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);

            var roles = await ObtenerRolesAsync(usuario.Id);
            var token = _jwtTokenService.GenerarToken(usuario, roles);
            return Ok(new TokenResponse(token, refreshTokenPlano));
        }

        [HttpPost("verify-mfa-recovery")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> VerifyMfaRecoveryCode(VerifyMfaRecoveryRequest request)
        {
            var idUsuario = _jwtTokenService.ValidarTokenMfaPending(request.MfaToken);
            if (idUsuario is null)
                return Unauthorized(new { mensaje = MensajeVerificacionInvalida });

            var usuario = await _userManager.FindByIdAsync(idUsuario.Value.ToString());
            if (usuario is null || !usuario.Activo)
                return Unauthorized(new { mensaje = MensajeVerificacionInvalida });

            if (await _userManager.IsLockedOutAsync(usuario))
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });

            var resultado = await _userManager.RedeemTwoFactorRecoveryCodeAsync(usuario, request.CodigoRecuperacion);
            if (!resultado.Succeeded)
            {
                await _userManager.AccessFailedAsync(usuario);
                await _auditoria.RegistrarAsync("VerificacionMfa", exitoso: false, idUsuario: usuario.Id, nombreUsuario: usuario.UserName, detalles: new { metodo = "RecoveryCode" });
                return Unauthorized(new { mensaje = "Código de recuperación inválido." });
            }

            await _userManager.ResetAccessFailedCountAsync(usuario);

            var (refreshTokenPlano, refreshTokenEntidad) = _refreshTokenService.Generar(usuario.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
            _db.RefreshTokens.Add(refreshTokenEntidad);
            await _db.SaveChangesAsync();

            await _auditoria.RegistrarAsync("VerificacionMfa", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName, detalles: new { metodo = "RecoveryCode" });
            await _auditoria.RegistrarAsync("Login", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);

            var roles = await ObtenerRolesAsync(usuario.Id);
            var token = _jwtTokenService.GenerarToken(usuario, roles);
            return Ok(new TokenResponse(token, refreshTokenPlano));
        }

        //reset
        [HttpPost("olvide-password")]
        [AllowAnonymous]
        public async Task<IActionResult> OlvidePassword([FromBody] OlvidePasswordRequest request)
        {
            var usuario = await _userManager.FindByEmailAsync(request.Email);
            if (usuario != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "https://localhost:44320";
                var emailCodificado = WebUtility.UrlEncode(usuario.Email);
                var tokenCodificado = WebUtility.UrlEncode(token);
                var linkReset = $"{frontendBaseUrl}/recuperar-password.html?email={emailCodificado}&token={tokenCodificado}";

                var cuerpoHtml = $"<p>Hola {usuario.Nombre},</p>" +
                                  "<p>Recibimos una solicitud para restablecer tu contraseña. " +
                                  "Si fuiste tú, haz clic en el siguiente enlace (válido por tiempo limitado):</p>" +
                                  $"<p><a href=\"{linkReset}\">Restablecer mi contraseña</a></p>" +
                                  "<p>Si no solicitaste esto, puedes ignorar este mensaje; tu contraseña actual seguirá funcionando.</p>";

                await _emailSender.EnviarAsync(usuario.Email!, $"{usuario.Nombre} {usuario.Apellido}", "Restablece tu contraseña", cuerpoHtml);
            }
            return Ok(new { mensaje = "Si el correo existe en nuestro sistema, te enviamos un enlace para restablecer tu contraseña" });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordTokenRequest request)
        {
            var usuario = await _userManager.FindByEmailAsync(request.Email);

            const string mensajeGenerico = "El enlace de restablecimiento no es válido o ha expirado.";

            if (usuario is null)
                return BadRequest(new { mensaje = mensajeGenerico });

            var resultado = await _userManager.ResetPasswordAsync(usuario, request.Token, request.PasswordNueva);
            if (!resultado.Succeeded)
                return BadRequest(new { mensaje = mensajeGenerico });

            usuario.DebeCambiarPassword = false;
            usuario.FechaModificacion = DateTime.UtcNow;
            await _userManager.UpdateAsync(usuario);

            await _auditoria.RegistrarAsync("CambioPassword", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);
            await RevocarTokenDeUsuarioAsync(usuario.Id);

            return Ok(new { mensaje = "Contraseña restablecida correctamente." });
        }

        [HttpPost("cambiar-password")]
        [Authorize]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordRequest request)
        {
            var idUsuario = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idUsuario is null) return Unauthorized();

            var usuario = await _userManager.FindByIdAsync(idUsuario);
            if (usuario is null || !usuario.Activo) return Unauthorized();

            if (await _userManager.IsLockedOutAsync(usuario))
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });

            var passwordValido = await _signInManager.CheckPasswordSignInAsync(usuario, request.PasswordActual, lockoutOnFailure: true);
            if (passwordValido.IsLockedOut)
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });
            if (!passwordValido.Succeeded)
                return Unauthorized(new { mensaje = "Contraseña actual incorrecta." });

            if (request.PasswordActual == request.PasswordNueva)
                return BadRequest(new { mensaje = "La nueva contraseña debe ser diferente a la actual." });

            var resultado = await _userManager.ChangePasswordAsync(usuario, request.PasswordActual, request.PasswordNueva);
            if (!resultado.Succeeded)
                return BadRequest(new { errores = resultado.Errors.Select(e => e.Description) });

            usuario.DebeCambiarPassword = false;
            usuario.FechaModificacion = DateTime.UtcNow;
            await _userManager.UpdateAsync(usuario);
            await _auditoria.RegistrarAsync("CambioPassword", exitoso: true, idUsuario: usuario.Id, nombreUsuario: usuario.UserName);
            await RevocarTokenDeUsuarioAsync(usuario.Id);
            return NoContent();
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> RefreshToken(RefreshTokenRequest request)
        {
            var hash = _refreshTokenService.Hash(request.RefreshToken);
            var guardado = await _db.RefreshTokens.Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (guardado is null || !guardado.EstaActivo || !guardado.Usuario.Activo)
                return Unauthorized(new { mensaje = "Refresh token inválido o expirado." });

            guardado.FechaRevocacion = DateTime.UtcNow;

            var (nuevoPlano, nuevaEntidad) = _refreshTokenService.Generar(guardado.IdUsuario, HttpContext.Connection.RemoteIpAddress?.ToString());
            guardado.ReemplazadoPorTokenHash = nuevaEntidad.TokenHash;
            _db.RefreshTokens.Add(nuevaEntidad);
            await _db.SaveChangesAsync();

            var roles = await ObtenerRolesAsync(guardado.IdUsuario);
            var jwt = _jwtTokenService.GenerarToken(guardado.Usuario, roles);

            return Ok(new TokenResponse(jwt, nuevoPlano));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(RefreshTokenRequest request)
        {
            var hash = _refreshTokenService.Hash(request.RefreshToken);
            var guardado = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (guardado is not null && guardado.FechaRevocacion is null)
            {
                guardado.FechaRevocacion = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return NoContent(); // idempotente, no revela si el token existía
        }

        private async Task<List<string>> ObtenerRolesAsync(int idUsuario)
        {
            var ahora = DateTime.UtcNow;

            return await _db.RolUsuario
                .Where(ru => ru.IdUsuario == idUsuario
                          && ru.Activo
                          && (ru.FechaFin == null || ru.FechaFin > ahora))
                .Join(_db.Roles.Where(r=>r.Activo), ru => ru.IdRol, r => r.IdRol, (ru, r) => r.Nombre)
                .ToListAsync();
        }
        private async Task RevocarTokenDeUsuarioAsync(int idUsuario)
        {
            var activos = await _db.RefreshTokens
                .Where(t => t.IdUsuario == idUsuario && t.FechaRevocacion == null)
                .ToListAsync();
            foreach (var t in activos)
                t.FechaRevocacion = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
