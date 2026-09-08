using System.Net.Mime;
using System.Text.Json;
using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Web.Middlewares;

/// <summary>
/// Tratamento centralizado de exceções, em vez de try/catch por controller.
/// Nenhum stack trace vaza para o cliente, toda falha é logada no mesmo
/// formato com identificador de correlação, e os controllers ficam apenas
/// com o caminho feliz. O corpo segue ProblemDetails (RFC 7807).
/// </summary>
public sealed partial class ExceptionHandlingMiddleware
{
    /// <summary>
    /// 499 é convenção do nginx e não existe em StatusCodes. Distinguir
    /// cancelamento de erro real evita poluir o log de 500 com ruído de
    /// usuário que trocou de tela no meio de uma consulta.
    /// </summary>
    private const int StatusClientClosedRequest = 499;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await TratarAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task TratarAsync(HttpContext context, Exception excecao)
    {
        // Correlaciona o que o usuário vê na tela com a linha no log.
        // Correlaciona o que o usuário vê na tela com a linha no log.
        var correlacao = context.TraceIdentifier;

        var (status, titulo, detalhe) = excecao switch
        {
            DomainException dominio =>
                (StatusCodes.Status400BadRequest, "Requisição inválida", dominio.Message),

            ConflitoDePersistenciaException conflito =>
                (StatusCodes.Status409Conflict, "Conflito", conflito.Message),

            OperationCanceledException =>
                (StatusClientClosedRequest, "Requisição cancelada", "A operação foi cancelada."),

            _ => (StatusCodes.Status500InternalServerError,
                  "Erro interno",
                  "Ocorreu um erro inesperado ao processar a requisição.")
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            LogFalhaNaoTratada(_logger, correlacao, excecao);
        }
        else
        {
            LogRequisicaoRejeitada(_logger, status, correlacao, excecao.Message);
        }

        if (context.Response.HasStarted)
        {
            // A resposta já começou a ser enviada; reescrever produziria
            // corpo corrompido.
            return;
        }

        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalhe,
            Instance = context.Request.Path
        };

        problema.Extensions["traceId"] = correlacao;

        // Stack trace apenas em Desenvolvimento: em produção, detalhe de
        // implementação no corpo do erro é vazamento de informação.
        if (_environment.IsDevelopment() && status >= StatusCodes.Status500InternalServerError)
        {
            problema.Extensions["excecao"] = excecao.ToString();
        }

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        await context.Response
            .WriteAsync(JsonSerializer.Serialize(problema, JsonOptions))
            .ConfigureAwait(false);
    }

    // [LoggerMessage] gera delegates tipados em tempo de compilação. A
    // chamada convencional recebe params object?[], o que aloca array e faz
    // boxing mesmo quando o nível de log está desabilitado - custo que
    // aparece sob carga num middleware que roda em toda requisição.

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message = "Falha nao tratada. Correlacao: {Correlacao}")]
    private static partial void LogFalhaNaoTratada(
        ILogger logger,
        string correlacao,
        Exception excecao);

    [LoggerMessage(
        EventId = 4000,
        Level = LogLevel.Warning,
        Message = "Requisicao rejeitada ({Status}). Correlacao: {Correlacao}. Motivo: {Motivo}")]
    private static partial void LogRequisicaoRejeitada(
        ILogger logger,
        int status,
        string correlacao,
        string motivo);
}
