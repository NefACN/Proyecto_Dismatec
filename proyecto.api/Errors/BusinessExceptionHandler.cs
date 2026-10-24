using Microsoft.AspNetCore.Diagnostics;
using proyect.application.Exceptions;

namespace proyecto.api.Errors;

public sealed class BusinessExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not BusinessRuleException error) return false;
        context.Response.StatusCode = error.Error switch
        {
            BusinessError.NotFound => StatusCodes.Status404NotFound,
            BusinessError.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
        await context.Response.WriteAsJsonAsync(new { mensaje = error.Message }, ct);
        return true;
    }
}
