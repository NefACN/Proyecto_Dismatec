using System.ComponentModel.DataAnnotations;

namespace proyect.application.Dtos
{
    public record IniciarAutenticatorResponse(string ClaveManual, string UriQr);

    public class ConfirmarAutenticatorRequest
    {
        [Required, StringLength(6, MinimumLength = 6), RegularExpression(@"^\d{6}$")]
        public string Codigo { get; set; } = string.Empty;
    }

    public record ConfirmarAutenticatorResponse(List<string> CodigoRecuperacion);

    public class DeshabilitarMfaRequest
    {
        [Required]
        public string PasswordActual { get; set; } = string.Empty;
    }

    public record EstadoMfaResponse(bool Habilitado, bool AuthenticatorConfigurado, int CodigosRecuperacionRestantes);
}
