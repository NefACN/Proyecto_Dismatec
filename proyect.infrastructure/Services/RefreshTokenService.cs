using proyect.application.Abstractions;
using Microsoft.Extensions.Configuration;
using proyect.domain.Entidades;
using System.Text;

namespace proyect.infrastructure.Services
{

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _config;
        public RefreshTokenService(IConfiguration config) => _config = config;

        public (string, RefreshToken) Generar(int idUsuario, string? ip)
        {
            var tokenPlano = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
            var dias = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");

            var entidad = new RefreshToken
            {
                IdUsuario = idUsuario,
                TokenHash = Hash(tokenPlano),
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddDays(dias),
                IpCreacion = ip
            };

            return (tokenPlano, entidad);
        }

        public string Hash(string tokenPlano)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(tokenPlano));
            return Convert.ToHexString(bytes);
        }
    }
}
