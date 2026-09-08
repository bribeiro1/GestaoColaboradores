using GestaoColaboradores.Domain.Repositories;
using GestaoColaboradores.Infrastructure.Persistence;
using GestaoColaboradores.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoColaboradores.Infrastructure.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // As migrations vivem nesta assembly, não na Web.
                sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);

                // Resiliência a falha transitória - timeout de rede, failover,
                // throttling. Sem isso, uma queda de milissegundos vira erro
                // para o usuário final.
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);

                sqlOptions.CommandTimeout(30);
            });
        });

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
