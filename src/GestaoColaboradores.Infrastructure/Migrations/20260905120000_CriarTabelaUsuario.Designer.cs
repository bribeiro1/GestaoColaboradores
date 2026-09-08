using System;
using GestaoColaboradores.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GestaoColaboradores.Infrastructure.Migrations;

/// <summary>
/// Modelo-alvo desta migration. Operações de dados (InsertData) não carregam
/// os tipos das colunas, então o EF precisa do modelo vigente neste ponto da
/// história do schema. Diferente do ModelSnapshot, que reflete o estado atual.
/// Arquivo mantido pelo tooling do EF.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20260905120000_CriarTabelaUsuario")]
partial class CriarTabelaUsuario
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("GestaoColaboradores.Domain.Entities.Usuario", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasColumnName("Id");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

            b.Property<bool>("Ativo")
                .HasColumnType("bit")
                .HasColumnName("Ativo");

            b.Property<DateTime>("DataCadastro")
                .HasColumnType("datetime2(3)")
                .HasColumnName("DataCadastro");

            b.Property<string>("Nome")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)")
                .HasColumnName("Nome");

            b.Property<decimal>("ValorHora")
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("ValorHora");

            b.HasKey("Id");

            b.HasIndex("Ativo")
                .HasDatabaseName("IX_Usuario_Ativo");

            b.HasIndex("Nome")
                .IsUnique()
                .HasDatabaseName("UX_Usuario_Nome");

            b.ToTable("Usuario", (string)null);
        });
#pragma warning restore 612, 618
    }
}
