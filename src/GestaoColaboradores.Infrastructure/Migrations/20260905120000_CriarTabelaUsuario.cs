using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoColaboradores.Infrastructure.Migrations;

/// <summary>
/// Criação da tabela Usuario. O script em /db é gerado a partir desta
/// migration com "dotnet ef migrations script --idempotent", então os dois
/// não podem divergir.
/// </summary>
public partial class CriarTabelaUsuario : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Usuario",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                ValorHora = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                DataCadastro = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                Ativo = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Usuario", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Usuario_Ativo",
            table: "Usuario",
            column: "Ativo");

        migrationBuilder.CreateIndex(
            name: "UX_Usuario_Nome",
            table: "Usuario",
            column: "Nome",
            unique: true);

        // InsertData imperativo em vez de HasData: HasData vincularia os
        // dados ao snapshot, e o EF tentaria sincronizar registros de
        // demonstração a cada migration futura.
        migrationBuilder.InsertData(
            table: "Usuario",
            columns: new[] { "Nome", "ValorHora", "DataCadastro", "Ativo" },
            values: new object[,]
            {
                { "Ana Carolina Souza", 185.00m, new DateTime(2026, 1, 15, 9, 30, 0), true },
                { "Bruno Almeida Lima", 240.50m, new DateTime(2026, 2, 3, 14, 0, 0), true },
                { "Carla Menezes Prado", 132.75m, new DateTime(2026, 3, 22, 8, 15, 0), false },
                { "Diego Fernandes Rocha", 310.00m, new DateTime(2026, 4, 8, 11, 45, 0), true },
                { "Eduarda Nogueira Alves", 97.30m, new DateTime(2026, 5, 30, 16, 20, 0), false }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Down funcional: migration sem rollback é deploy sem plano de volta.
        migrationBuilder.DropTable(name: "Usuario");
    }
}
