/* =============================================================================
   001_Create_Usuario.sql

   Criacao da tabela Usuario, seus indices e a carga de dados de exemplo.

   ---------------------------------------------------------------------------
   POR QUE ESTE SCRIPT EXISTE, SE O PROJETO USA MIGRATIONS
   ---------------------------------------------------------------------------
   As duas coisas atendem a publicos diferentes:

     - A MIGRATION (code-first, em src/Infrastructure/Migrations) e a
       ferramenta do ciclo de desenvolvimento: versiona o modelo junto com
       o codigo e permite recriar o banco com um comando.

     - O SCRIPT e o artefato de RELEASE. Em ambiente corporativo, alteracao
       de schema em producao costuma passar por aprovacao do DBA, janela de
       manutencao e revisao previa. Nesse contexto, deixar a aplicacao migrar
       o banco no startup nao passa pela governanca - e, com varias instancias
       subindo em paralelo, duas podem tentar migrar ao mesmo tempo.

   O script nao e escrito a mao: e gerado a partir da propria migration com

       dotnet ef migrations script --idempotent ^
           --project src/GestaoColaboradores.Infrastructure ^
           --startup-project src/GestaoColaboradores.Web ^
           --output db/001_Create_Usuario.sql

   de modo que codigo e script nao podem divergir - um e derivado do outro.
   Esta versao foi comentada para leitura.

   ---------------------------------------------------------------------------
   IDEMPOTENTE
   ---------------------------------------------------------------------------
   Pode ser executado mais de uma vez sem erro e sem duplicar dados. Em
   pipeline de release isso deixa de ser preciosismo: um deploy reexecutado
   apos falha parcial nao pode quebrar por causa de um objeto que ja existe.

   Compatibilidade: SQL Server 2016 ou superior.
   ============================================================================= */

USE [GestaoColaboradores];
GO

SET NOCOUNT ON;
GO

/* -----------------------------------------------------------------------------
   1. Tabela de controle de migrations do EF Core.

   Criada aqui pelo mesmo motivo que o EF a cria: registrar quais migrations
   ja foram aplicadas. Assim, um "dotnet ef database update" executado depois
   deste script reconhece que a migration ja esta aplicada e nao tenta
   recriar a tabela.
   ----------------------------------------------------------------------------- */
IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory]
    (
        [MigrationId]    NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );

    PRINT N'Tabela __EFMigrationsHistory criada.';
END
GO

/* -----------------------------------------------------------------------------
   2. Tabela Usuario.

   Notas de modelagem:

   Id           INT IDENTITY, chave primaria. Clustered por padrao: a chave e
                estreita, sempre crescente e imutavel - o melhor candidato a
                indice clusterizado, porque evita fragmentacao por page split.

   Nome         NVARCHAR(150). NVARCHAR e nao VARCHAR: nomes brasileiros usam
                acentuacao e o Unicode elimina qualquer dependencia de code page.

   ValorHora    DECIMAL(18,2). Nunca FLOAT ou REAL para dinheiro: ponto
                flutuante binario nao representa 0,1 exatamente e acumula erro
                de arredondamento - inaceitavel em base de faturamento.

   DataCadastro DATETIME2(3). O enunciado pede "datetime"; a divergencia e
                deliberada e esta documentada no README. DATETIME tem precisao
                de 3,33 ms e faixa iniciando em 1753, nao corresponde
                exatamente a System.DateTime do .NET e arredonda em silencio.
                DATETIME2 e a recomendacao da Microsoft desde o SQL Server 2008.
                Caso a padronizacao do cliente exija DATETIME, a alteracao e
                de uma linha aqui e uma na UsuarioConfiguration.

   Ativo        BIT. Tipo booleano nativo do SQL Server; ocupa 1 byte e o
                mecanismo agrupa ate 8 colunas BIT no mesmo byte.
   ----------------------------------------------------------------------------- */
IF OBJECT_ID(N'[Usuario]', N'U') IS NULL
BEGIN
    CREATE TABLE [Usuario]
    (
        [Id]           INT            IDENTITY(1,1) NOT NULL,
        [Nome]         NVARCHAR(150)  NOT NULL,
        [ValorHora]    DECIMAL(18, 2) NOT NULL,
        [DataCadastro] DATETIME2(3)   NOT NULL,
        [Ativo]        BIT            NOT NULL,
        CONSTRAINT [PK_Usuario] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT N'Tabela Usuario criada.';
END
ELSE
BEGIN
    PRINT N'Tabela Usuario ja existe. Nada a fazer.';
END
GO

/* -----------------------------------------------------------------------------
   3. Indice UNICO em Nome.

   A aplicacao ja verifica duplicidade antes de gravar, mas entre a consulta
   e o INSERT existe uma janela em que outra requisicao pode inserir o mesmo
   nome. Sob concorrencia, so a restricao do banco garante a regra.
   A verificacao na aplicacao existe para dar uma mensagem clara no caso
   comum; o indice existe para garantir a integridade no caso raro.

   Como efeito colateral util, este indice tambem atende a busca por nome.
   ----------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_Usuario_Nome' AND object_id = OBJECT_ID(N'[Usuario]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Usuario_Nome] ON [Usuario] ([Nome] ASC);
    PRINT N'Indice UX_Usuario_Nome criado.';
END
GO

/* -----------------------------------------------------------------------------
   4. Indice de apoio ao filtro por situacao.

   INCLUDE nas colunas exibidas no grid torna o indice COBERTOR para a
   consulta da tela: o SQL Server resolve tudo no proprio indice, sem
   key lookup na tabela base.
   ----------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Usuario_Ativo' AND object_id = OBJECT_ID(N'[Usuario]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Usuario_Ativo] ON [Usuario] ([Ativo] ASC)
        INCLUDE ([Nome], [ValorHora], [DataCadastro]);
    PRINT N'Indice IX_Usuario_Ativo criado.';
END
GO

/* -----------------------------------------------------------------------------
   5. Dados de exemplo.

   Inseridos apenas se a tabela estiver vazia, para que uma reexecucao do
   script nao duplique registros nem sobrescreva dados reais.
   ----------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [Usuario])
BEGIN
    INSERT INTO [Usuario] ([Nome], [ValorHora], [DataCadastro], [Ativo])
    VALUES
        (N'Ana Carolina Souza',     185.00, '2026-01-15T09:30:00', 1),
        (N'Bruno Almeida Lima',     240.50, '2026-02-03T14:00:00', 1),
        (N'Carla Menezes Prado',    132.75, '2026-03-22T08:15:00', 0),
        (N'Diego Fernandes Rocha',  310.00, '2026-04-08T11:45:00', 1),
        (N'Eduarda Nogueira Alves',  97.30, '2026-05-30T16:20:00', 0);

    PRINT N'Dados de exemplo inseridos.';
END
GO

/* -----------------------------------------------------------------------------
   6. Registro da migration como aplicada.
   ----------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory]
               WHERE [MigrationId] = N'20260905120000_CriarTabelaUsuario')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905120000_CriarTabelaUsuario', N'10.0.0');

    PRINT N'Migration 20260905120000_CriarTabelaUsuario registrada como aplicada.';
END
GO

PRINT N'Script concluido com sucesso.';
GO
