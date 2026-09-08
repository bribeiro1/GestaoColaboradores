namespace GestaoColaboradores.Application.DTOs;

/// <summary>
/// Corpo do PATCH de situação. Tipo próprio em vez de um bool na query
/// string, para o contrato crescer sem quebrar a assinatura do endpoint.
/// </summary>
public sealed class AlterarSituacaoRequest
{
    public bool Ativo { get; set; }
}
