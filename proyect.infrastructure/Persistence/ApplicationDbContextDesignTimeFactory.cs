using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using proyect.infrastructure.Configuration;

namespace proyect.infrastructure.Persistence;

public sealed class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddProjectEnvironmentFile(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables().Build();
        var connection = configuration["ConnectionStrings:PostgreSQL"]
            ?? "Host=127.0.0.1;Port=5433;Database=DISMATEC;Username=postgres";
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connection).Options);
    }
}
