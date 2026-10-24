using proyect.domain.Entidades;

namespace proyect.application.Abstractions
{
    public interface ISmsSender
    {
        Task EnviarAsync(string numeroDestino, string mensaje);
    }
}
