using Microsoft.AspNetCore.Identity;
using proyect.application.Abstractions;
using proyect.domain.Entidades;

namespace proyect.infrastructure.Services;

public sealed class UserSessionInvalidator(UserManager<ApplicationUser> users) : IUserSessionInvalidator
{
    public async Task InvalidarAsync(int idUsuario)
    {
        var user = await users.FindByIdAsync(idUsuario.ToString());
        if (user is null) return;
        var result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded) throw new InvalidOperationException("No se pudieron invalidar las sesiones del usuario.");
    }
}
