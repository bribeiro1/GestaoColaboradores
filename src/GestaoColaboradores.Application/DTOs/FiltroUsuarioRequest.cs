namespace GestaoColaboradores.Application.DTOs;

/// <summary>Filtros opcionais da consulta de usuários.</summary>
public sealed class FiltroUsuarioRequest
{
    public string? Nome { get; set; }

    /// <summary>Nulo = todos; true = ativos; false = inativos.</summary>
    public bool? Ativo { get; set; }
}
