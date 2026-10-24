using proyect.application.Abstractions;
using Microsoft.Extensions.Configuration;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.Clients;

namespace proyect.infrastructure.Services
{
    public class TwilioSmsSender : ISmsSender
    {
        private readonly IConfiguration _config;

        public TwilioSmsSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarAsync(string numeroDestino, string mensaje)
        {
            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];
            var origen = _config["Twilio:NumeroOrigen"];
            if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(origen))
                throw new InvalidOperationException("El proveedor SMS no está configurado.");

            await MessageResource.CreateAsync(
                to: new PhoneNumber(numeroDestino),
                from: new PhoneNumber(origen),
                body: mensaje,
                client: new TwilioRestClient(accountSid, authToken));
        }
    }
}
