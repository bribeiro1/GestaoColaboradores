/* =============================================================================
   000_Create_Database.sql
   Criacao do banco de dados. Executar conectado ao banco master.

   Separado do script de schema porque, em ambiente corporativo, criar o
   banco costuma ser atribuicao do DBA e acontece uma unica vez - enquanto
   os scripts de schema rodam a cada release.
   ============================================================================= */

IF DB_ID(N'GestaoColaboradores') IS NULL
BEGIN
    PRINT N'Criando o banco de dados GestaoColaboradores...';
    CREATE DATABASE [GestaoColaboradores];
END
ELSE
BEGIN
    PRINT N'O banco de dados GestaoColaboradores ja existe. Nada a fazer.';
END
GO
