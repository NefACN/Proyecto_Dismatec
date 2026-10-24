using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using proyect.domain.Entidades;
using proyect.application.Dtos;
using proyect.application.Security;

namespace proyecto.api.Controllers
{
    [ApiController]
    [Route("api/mfa")]
    [Authorize]
    public class MfaController : ControllerBase
    {
        private const string FormatoUriAutenticator = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public MfaController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }
        [HttpGet("estado")]
        public async Task<ActionResult> VerificarEstadoMfa()
        {
            var usuario = await ObtenerCurrentUserAsync();
            if (usuario is null) return Unauthorized();
            
            var habilitado = await _userManager.GetTwoFactorEnabledAsync(usuario);
            //var authenticatorConfigurado = !string.IsNullOrEmpty(await _userManager.GetAuthenticatorKeyAsync(usuario));
            var codigosRecuperacionRestantes = await _userManager.CountRecoveryCodesAsync(usuario);
            return Ok(new EstadoMfaResponse(habilitado, habilitado, codigosRecuperacionRestantes ));
        }
        [HttpPost("autenticator/iniciar")]
        public async Task<ActionResult<IniciarAutenticatorResponse>> IniciarAutenticador()
        {
            var usuario = await ObtenerCurrentUserAsync();
            if (usuario is null) return Unauthorized();

            if (await _userManager.GetTwoFactorEnabledAsync(usuario))
                return Conflict(new { mensaje = "Deshabilita el autenticador actual antes de configurar uno nuevo." });

            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            var clave = await _userManager.GetAuthenticatorKeyAsync(usuario);

            if (string.IsNullOrEmpty(clave))
                return StatusCode(500, new { mensaje = "No se pudo generar la clave" });

            var emisor = _config["Jwt:Issuer"] ?? "Proyecto";
            var uriQr = string.Format(
                FormatoUriAutenticator,
                UrlEncoder.Default.Encode(emisor),
                UrlEncoder.Default.Encode(usuario.Email ?? usuario.UserName ?? "usuario"),
                clave);

            return Ok(new IniciarAutenticatorResponse(ClaveManual: AuthenticatorKeyFormatter.Format(clave), UriQr: uriQr));
        }

        [HttpPost("autenticator/confirmar")]
        public async Task<ActionResult<ConfirmarAutenticatorResponse>> ConfirmarAutenticador(ConfirmarAutenticatorRequest request)
        {
            var usuario = await ObtenerCurrentUserAsync();
            if (usuario is null) return Unauthorized();

            var esValido = await _userManager.VerifyTwoFactorTokenAsync(
                usuario, TokenOptions.DefaultAuthenticatorProvider, request.Codigo);

            if (!esValido)
                return BadRequest(new { mensaje = "El reloj no esta sincronizado" });

            await _userManager.SetTwoFactorEnabledAsync(usuario, true);

            var codigosRecuperacion = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(usuario, 10);

            return Ok(new ConfirmarAutenticatorResponse(codigosRecuperacion?.ToList() ?? new List<string>()));
        }
        [HttpPost("autenticator/quitar")]
        public async Task<IActionResult> QuitarAutenticador(DeshabilitarMfaRequest request)
        {
            var usuario = await ObtenerCurrentUserAsync();
            if (usuario is null) return Unauthorized();

            var passwordValido = await _signInManager.CheckPasswordSignInAsync(usuario, request.PasswordActual, lockoutOnFailure: true);
            if (passwordValido.IsLockedOut)
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });
            if (!passwordValido.Succeeded)
                return Unauthorized(new { mensaje = "Contraseña incorrecta." });

            await _userManager.SetTwoFactorEnabledAsync(usuario, false);
            
            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            
            //await _userManager.UpdateSecurityStampAsync(usuario);

            return NoContent();
        }

        [HttpPost("regenerar-codigos-recuperacion")]
        public async Task<ActionResult<ConfirmarAutenticatorResponse>> RegenerarCodigosRecuperacion(DeshabilitarMfaRequest request)
        {
            var usuario = await ObtenerCurrentUserAsync();
            if (usuario is null) return Unauthorized();

            var authenticatorConfigurado = !string.IsNullOrEmpty(await _userManager.GetAuthenticatorKeyAsync(usuario));
            if (!authenticatorConfigurado)
                return BadRequest(new { mensaje = "Primero debes configurar tu app autenticadora." });

            var passwordValido = await _signInManager.CheckPasswordSignInAsync(usuario, request.PasswordActual, lockoutOnFailure: true);
            if (passwordValido.IsLockedOut)
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });
            if (!passwordValido.Succeeded)
                return Unauthorized(new { mensaje = "Contraseña incorrecta." });

            var codigosRecuperacion = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(usuario, 10);
            return Ok(new ConfirmarAutenticatorResponse(codigosRecuperacion?.ToList() ?? new List<string>()));
        }
        private async Task<ApplicationUser?> ObtenerCurrentUserAsync()
        {
            var idUsuario = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return idUsuario is null ? null : await _userManager.FindByIdAsync(idUsuario);
        }

    }
}
