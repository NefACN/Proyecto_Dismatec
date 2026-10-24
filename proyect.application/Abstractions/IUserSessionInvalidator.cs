namespace proyect.application.Abstractions;

public interface IUserSessionInvalidator
{
    Task InvalidarAsync(int idUsuario);
}
