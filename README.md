# Gestão de Colaboradores

Aplicação web ASP.NET Core MVC para gestão de usuários e valor/hora, sobre SQL Server, Entity Framework Core e jQuery.Ajax.

## Stack

.NET 10 (LTS) · ASP.NET Core MVC + Razor · Entity Framework Core 10 (code-first) · SQL Server 2022 · jQuery 3.7 · Bootstrap 5.3 · xUnit + NSubstitute

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker Desktop **ou** SQL Server LocalDB **ou** uma instância de SQL Server

## Como executar

### Banco em Docker, aplicação na IDE

```bash
docker compose up -d sqlserver
dotnet run --project src/GestaoColaboradores.Web
```

Aplicação em **https://localhost:7189**. Em `Development` as migrations são aplicadas no startup e o banco sobe com registros de exemplo.

### Sem Docker (LocalDB)

Ajuste a connection string em `src/GestaoColaboradores.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=GestaoColaboradores;Trusted_Connection=True;MultipleActiveResultSets=True"
  }
}
```

### Tudo em contêiner

```bash
docker compose --profile completo up -d      # aplicação em http://localhost:8080
```

### Aplicando o schema pelo script SQL

```bash
sqlcmd -S localhost,1433 -U sa -P "Str0ng!Passw0rd" -C -i db/000_Create_Database.sql
sqlcmd -S localhost,1433 -U sa -P "Str0ng!Passw0rd" -C -i db/001_Create_Usuario.sql
```

Depois defina `"Banco:AplicarMigrationsNoStartup": false`. O script registra a migration em `__EFMigrationsHistory`, então o EF reconhece o schema como aplicado.

### Bibliotecas de front-end

Servidas de `wwwroot/lib` via LibMan (`libman.json`), com fallback automático para CDN. O Visual Studio restaura sozinho.

### Testes e documentação da API

```bash
dotnet test
```

Com a aplicação rodando: **`/scalar`** para explorar a API, **`/openapi/v1.json`** para o documento. O arquivo `GestaoColaboradores.Web.http` traz as chamadas prontas e roda dentro do Visual Studio.

---

## Estrutura

```
src/
├── GestaoColaboradores.Domain/          entidades, invariantes, contratos de repositório
├── GestaoColaboradores.Application/     casos de uso, DTOs, Result
├── GestaoColaboradores.Infrastructure/  EF Core, repositórios, migrations
└── GestaoColaboradores.Web/             MVC, API, views, middlewares
tests/  db/  docker-compose.yml
```

As dependências apontam para dentro: o `Domain` não referencia nada, e a `Infrastructure` depende dele para **implementar** as interfaces que o domínio declara.

## Decisões

| Decisão | Motivo |
|---|---|
| Entidade com setters privados e invariantes internas | Garante o estado válido para qualquer caminho de entrada, não só pela tela. |
| `IUnitOfWork` separado do repositório | O caso de uso decide o limite da transação, e não cada repositório por conta própria. |
| Dois controllers (views e API) | Contratos de erro diferentes: página de erro versus `ProblemDetails`. |
| DataAnnotations no DTO de entrada | O Razor emite os `data-val-*` para o jquery.validate e o `ModelState` revalida: uma declaração, validação nos dois lados. |
| Migration **e** script SQL idempotente | Migration para o ciclo de desenvolvimento; script para futuro release, onde schema passa por aprovação e janela. O script é gerado da migration, então não podem divergir. |
| Índice único em `Nome`, além da validação na aplicação | A validação dá a mensagem boa; o índice garante a integridade na janela entre a consulta e o INSERT. |

**Segurança:** antiforgery global com token por header nas chamadas AJAX; escape de HTML no grid montado em JavaScript; consultas parametrizadas pelo EF; stack trace nunca chega ao cliente.

## Endpoints

| Verbo | Rota | Respostas |
|---|---|---|
| `GET` | `/api/usuarios?nome=&ativo=` | 200 |
| `GET` | `/api/usuarios/{id}` | 200, 404 |
| `POST` | `/api/usuarios` | 201, 400, 409 |
| `PUT` | `/api/usuarios/{id}` | 200, 400, 404, 409 |
| `DELETE` | `/api/usuarios/{id}` | 204, 404 |
| `PATCH` | `/api/usuarios/{id}/situacao` | 200, 404 |

Erros seguem ProblemDetails (RFC 7807), com o dicionário `errors` por campo nas falhas de validação.

## Testes

40 testes. O grosso são unitários — invariantes do agregado e orquestração dos casos de uso. Os de integração cobrem só o que não existe sem banco real (tradução de consulta e constraints) e rodam em SQLite em memória.

## Fora do escopo

Paginação no servidor, concorrência otimista via `rowversion` (exigiria uma coluna além das especificadas), autenticação (sem um Entra ID real para integrar), eventos de integração (não há consumidor no escopo), observabilidade e pipeline CI/CD.
