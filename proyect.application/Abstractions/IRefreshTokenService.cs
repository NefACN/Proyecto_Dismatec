using proyect.domain.Entidades;

namespace proyect.application.Abstractions
{
    public interface IRefreshTokenService
    {
        (string tokenPlano, RefreshToken entidad) Generar(int idUsuario, string? ip);
        string Hash(string tokenPlano);
    }
}
