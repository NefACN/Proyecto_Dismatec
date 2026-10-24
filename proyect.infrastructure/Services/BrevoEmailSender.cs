using proyect.application.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
namespace proyect.infrastructure.Services
{
    public class BrevoEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public BrevoEmailSender(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task EnviarAsync(string destinatarioEmail, string destinatarioNombre, string asunto, string cuerpoHtml)
        {
            if (string.IsNullOrWhiteSpace(_config["Brevo:ApiKey"]) || string.IsNullOrWhiteSpace(_config["Brevo:EmailRemitente"]))
                throw new InvalidOperationException("El proveedor de correo no está configurado.");
            var payload = new
            {
                sender = new
                {
                    name = _config["Brevo:NombreRemitente"],
                    email = _config["Brevo:EmailRemitente"]
                },
                to = new[] { new { email = destinatarioEmail, name = destinatarioNombre } },
                subject = asunto,
                htmlContent = cuerpoHtml
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("api-key", _config["Brevo:ApiKey"]);
            request.Headers.Add("accept", "application/json");

            using var respuesta = await _httpClient.SendAsync(request);

            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Brevo no pudo enviar el correo ({(int)respuesta.StatusCode}): {detalle}");
            }
        }
    }
}
