using proyect.application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using proyect.domain.Entidades;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using proyect.application.Security;

namespace proyect.infrastructure.Services
{

    public class JwtTokenService : IJwtTokenService
    {
        private const string PropositoMfaPendiente = "mfa_pendiente";
        private const string PropositoCambioPassword = "cambio_password_obligatorio";
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerarToken(ApplicationUser usuario, IEnumerable<string> roles)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
                new(ClaimTypes.Name, usuario.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtClaim.SecurityStamp, usuario.SecurityStamp ?? string.Empty),
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var expiryMins = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");
            var token = new JwtSecurityToken(
                    issuer: jwtSection["Issuer"],
                    audience: jwtSection["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMins),
                    signingCredentials: credenciales
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerarTokenMfaPending(ApplicationUser usuario) =>
        GenerarTokenProposito(usuario, PropositoMfaPendiente, minutosValidez: 5);

        public int? ValidarTokenMfaPending(string token) =>
            ValidarTokenProposito(token, PropositoMfaPendiente);

        public string GenerarTokenCambioPasswordObligatorio(ApplicationUser usuario) =>
            GenerarTokenProposito(usuario, PropositoCambioPassword, minutosValidez: 10);

        public int? ValidarTokenCambioPasswordObligatorio(string token) =>
            ValidarTokenProposito(token, PropositoCambioPassword);

        private string GenerarTokenProposito(ApplicationUser usuario, string proposito, int minutosValidez)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new("purpose", proposito)
        };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutosValidez),
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int? ValidarTokenProposito(string token, string propositoEsperado)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromSeconds(30)
                }, out _);

                var proposito = principal.FindFirstValue("purpose");
                if (proposito != propositoEsperado) return null;

                var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                return sub is not null ? int.Parse(sub) : null;
            }
            catch
            {
                return null; // token expirado o invalido
            }
        }
    }
}
