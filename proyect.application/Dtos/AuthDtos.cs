using System.ComponentModel.DataAnnotations;

namespace proyect.application.Dtos
{
    public record LoginRequest
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }

    public record LoginResponse(
        bool RequiereCambioPassword,
        string? CambioPasswordToken,
        bool RequiereMfa,
        string? Token,
        string? Refreshtoken,
        string? MfaToken,
        List<string>? MetodosDisponibles);

    public class CambiarPasswordObligatorioRequest
    {
        [Required]
        public string CambioPasswordToken { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string PasswordNueva { get; set; } = string.Empty;
    }

    public class CambiarPasswordRequest
    {
        [Required]
        public string PasswordActual { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string PasswordNueva { get; set; } = string.Empty;
    }
    public class EnviarCodigoMfaRequest
    {
        [Required]
        public string MfaToken { get; set; } = string.Empty;

        [Required]
        public string Metodo { get; set; } = string.Empty;
    }

    public class VerifyMfaRequest
    {
        [Required]
        public string MfaToken { get; set; } = string.Empty;

        [Required]
        public string Metodo { get; set; } = string.Empty;

        [Required, StringLength(10, MinimumLength = 6)]
        public string Codigo { get; set; } = string.Empty;
    }
    public class VerifyMfaRecoveryRequest
    {
        [Required]
        public string MfaToken { get; set; } = string.Empty;

        [Required]
        public string CodigoRecuperacion { get; set; } = string.Empty;
    }

    public record TokenResponse(string Token, string RefreshToken);
    public record RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
    public record OlvidePasswordRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;
    }
    public class ResetPasswordTokenRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string PasswordNueva { get; set; } = string.Empty;
    }
}
