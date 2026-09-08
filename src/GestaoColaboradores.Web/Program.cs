using System.Globalization;
using GestaoColaboradores.Application;
using GestaoColaboradores.Infrastructure.DependencyInjection;
using GestaoColaboradores.Infrastructure.Persistence;
using GestaoColaboradores.Web.Middlewares;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Cultura pt-BR
// ---------------------------------------------------------------------------
// A aplicacao lida com moeda e data no padrao brasileiro. Fixar a cultura
// evita que a formatacao dependa da configuracao regional do servidor -
// causa classica de "funciona na minha maquina e quebra em producao",
// especialmente com separador decimal (virgula x ponto).
var culturaPtBr = new CultureInfo("pt-BR");
var opcoesLocalizacao = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaPtBr),
    SupportedCultures = [culturaPtBr],
    SupportedUICultures = [culturaPtBr]
};

// ---------------------------------------------------------------------------
// Servicos
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException(
        "Connection string 'SqlServer' não configurada. Veja o README.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddControllersWithViews(options =>
{
    // Validacao automatica de antiforgery em todo verbo que altera estado
    // (POST, PUT, PATCH, DELETE), sem precisar decorar cada action.
    // Aplicado globalmente porque a protecao correta e a que nao depende de
    // alguem lembrar de escrever o atributo na proxima action criada.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAntiforgery(options =>
{
    // As chamadas jQuery.ajax enviam o token por header, ja que o corpo
    // da requisicao e JSON e nao um form urlencoded.
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ---------------------------------------------------------------------------
// Documentacao da API (OpenAPI)
// ---------------------------------------------------------------------------
// Geracao nativa do .NET 9+. O Swashbuckle saiu dos templates do ASP.NET Core
// e a geracao do documento passou a ser parte do framework.
// Os atributos [ProducesResponseType] dos controllers alimentam o documento,
// entao os status possiveis de cada endpoint ficam descritos sem trabalho extra.
builder.Services.AddOpenApi(options =>
{
    // Sem isto o documento herda o nome do assembly como título.
    options.AddDocumentTransformer((documento, _, _) =>
    {
        documento.Info = new()
        {
            Title = "Gestão de Colaboradores - API",
            Version = "v1",
            Description =
                "Endpoints de gestão de usuários e valor/hora, consumidos por " +
                "jQuery.ajax nas telas de consulta e cadastro.\n\n" +
                "Erros seguem ProblemDetails (RFC 7807). Falhas de validação " +
                "incluem o dicionário `errors` com as mensagens por campo.\n\n" +
                "**Atenção:** os verbos que alteram estado exigem o header " +
                "`RequestVerificationToken` (proteção antiforgery), portanto " +
                "retornam 400 quando disparados diretamente por esta página."
        };

        return Task.CompletedTask;
    });
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline
// ---------------------------------------------------------------------------

// Primeiro da fila, para capturar excecoes de todo o restante do pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRequestLocalization(opcoesLocalizacao);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();   // /openapi/v1.json

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Gestao de Colaboradores - API");
    });
}
else
{
    app.UseExceptionHandler("/Home/Erro");
    app.UseHsts();

    // A documentação não é publicada em produção: um OpenAPI aberto entrega
    // o mapa da superfície de ataque. Em ambiente corporativo vai para portal
    // interno ou fica atrás de autenticação.
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Index}/{id?}");

// Migration no startup apenas em Desenvolvimento e sob flag explícita.
// Em produção o schema vai por script versionado no pipeline (ver /db):
// migração disparada pela aplicação ignora janela e aprovação, e com várias
// instâncias subindo em paralelo duas podem tentar migrar ao mesmo tempo.
if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue<bool>("Banco:AplicarMigrationsNoStartup"))
{
    using var escopo = app.Services.CreateScope();
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await contexto.Database.MigrateAsync();
}

await app.RunAsync();
