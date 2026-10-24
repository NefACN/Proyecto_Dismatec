using proyect.domain.Entidades;

namespace proyect.application.Abstractions
{
    public interface IJwtTokenService
    {
        string GenerarToken(ApplicationUser usuario, IEnumerable<string> roles);

        //completar el segundo paso del loging
        string GenerarTokenMfaPending(ApplicationUser usuario);
        // comes back the id of valid user token 
        int? ValidarTokenMfaPending(string token);

        string GenerarTokenCambioPasswordObligatorio(ApplicationUser usuario);
        int? ValidarTokenCambioPasswordObligatorio(string token);
    }
}
