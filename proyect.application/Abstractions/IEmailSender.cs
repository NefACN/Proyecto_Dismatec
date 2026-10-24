using proyect.domain.Entidades;

namespace proyect.application.Abstractions
{
    public interface IEmailSender
    {
        Task EnviarAsync(string destinatarioEmail, string destinatarioNombre, string asunto, string cuerpoHtml);
    }
}
