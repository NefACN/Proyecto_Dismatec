using Microsoft.AspNetCore.Http;
using proyect.application.Abstractions;

namespace proyect.infrastructure.Services;

internal sealed class HttpRequestContext(IHttpContextAccessor accessor) : IRequestContext
{
    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
