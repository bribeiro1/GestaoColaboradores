using GestaoColaboradores.Application.UseCases.Usuarios;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoColaboradores.Application;

/// <summary>
/// Registro dos serviços da camada de aplicação. Cada camada expõe o próprio
/// método de registro, evitando um projeto central que conheça todas elas.
/// </summary>
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Substituído por um relógio fixo nos testes.
        services.AddSingleton(TimeProvider.System);

        // Scoped: acompanha o tempo de vida da request e do DbContext.
        services.AddScoped<IListarUsuariosUseCase, ListarUsuariosUseCase>();
        services.AddScoped<IObterUsuarioUseCase, ObterUsuarioUseCase>();
        services.AddScoped<ICriarUsuarioUseCase, CriarUsuarioUseCase>();
        services.AddScoped<IAtualizarUsuarioUseCase, AtualizarUsuarioUseCase>();
        services.AddScoped<IExcluirUsuarioUseCase, ExcluirUsuarioUseCase>();
        services.AddScoped<IAlterarSituacaoUsuarioUseCase, AlterarSituacaoUsuarioUseCase>();

        return services;
    }
}
