using Microsoft.Extensions.DependencyInjection;
using proyect.application.Abstractions;
using proyect.application.Services;

namespace proyect.application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<AreasOperativasService>();
        services.AddScoped<SucursalesService>();
        services.AddScoped<CiudadesService>();
        services.AddScoped<AuditoriaConsultaService>();
        services.AddScoped<RolesService>();
        return services;
    }
}
