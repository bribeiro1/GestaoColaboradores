using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Domain.Entities;

namespace GestaoColaboradores.Application.UseCases.Usuarios;

/// <summary>
/// Mapeamento manual entre entidade e DTO. Sem AutoMapper: com um agregado e
/// cinco campos, o compilador é a melhor rede de segurança - se um campo
/// mudar, o build quebra em vez de falhar em execução.
/// </summary>
internal static class UsuarioMapper
{
    public static UsuarioDto ParaDto(this Usuario usuario) => new(
        usuario.Id,
        usuario.Nome,
        usuario.ValorHora,
        usuario.DataCadastro,
        usuario.Ativo);

    public static IReadOnlyList<UsuarioDto> ParaDto(this IEnumerable<Usuario> usuarios) =>
        usuarios.Select(ParaDto).ToList();
}
