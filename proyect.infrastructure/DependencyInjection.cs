using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using proyect.application.Security;
using proyect.application.Abstractions;
using proyect.domain.Entidades;
using proyect.infrastructure.Persistence;
using proyect.infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace proyect.infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Configura ConnectionStrings:PostgreSQL mediante secretos de usuario o variables de entorno.");
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            throw new InvalidOperationException("Configura Jwt:Key con una clave de al menos 32 bytes.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connection));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager();

        var jwtSection = configuration.GetSection("Jwt");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Key"]!)),

                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices
                            .GetRequiredService<UserManager<ApplicationUser>>();

                        var idUsuario = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                                     ?? context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (idUsuario is null)
                        {
                            context.Fail("Token sin identidad de usuario.");
                            return;
                        }

                        var usuario = await userManager.FindByIdAsync(idUsuario);
                        if (usuario is null || !usuario.Activo)
                        {
                            context.Fail("Usuario inactivo o inexistente.");
                            return;
                        }

                        var stampEnToken = context.Principal!.FindFirstValue(JwtClaim.SecurityStamp);
                        if (string.IsNullOrEmpty(stampEnToken) || stampEnToken != usuario.SecurityStamp)
                        {
                            context.Fail("La sesión ya no es válida, vuelve a iniciar sesión.");
                            return;
                        }
                    }
                };
            });

        services.AddAuthorization();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddHttpClient<IEmailSender, BrevoEmailSender>();
        services.AddScoped<ISmsSender, TwilioSmsSender>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContext, HttpRequestContext>();
        services.AddScoped<IUserSessionInvalidator, UserSessionInvalidator>();
        return services;
    }
}
