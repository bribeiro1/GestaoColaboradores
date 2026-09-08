using GestaoColaboradores.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoColaboradores.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento de Usuario por Fluent API, sem atributos de EF na entidade -
/// trocar de ORM alteraria apenas este arquivo.
/// </summary>
public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // Singular e explícito, conforme o enunciado: por convenção o EF
        // pluralizaria para "Usuarios".
        builder.ToTable("Usuario");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(UsuarioLimites.NomeTamanhoMaximo)
            .IsRequired();

        // HasPrecision explícito: sem ele o EF avisa que a precisão não foi
        // definida, e uma mudança de convenção truncaria valor monetário.
        builder.Property(u => u.ValorHora)
            .HasColumnName("ValorHora")
            .HasPrecision(UsuarioLimites.ValorHoraPrecisao, UsuarioLimites.ValorHoraEscala)
            .IsRequired();

        // Divergência consciente do enunciado, que pede "datetime": datetime
        // tem precisão de 3,33 ms, não corresponde exatamente a System.DateTime
        // e arredonda em silêncio. Ver a seção de divergências do README.
        builder.Property(u => u.DataCadastro)
            .HasColumnName("DataCadastro")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(u => u.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        // A validação no caso de uso resolve a experiência do usuário; este
        // índice garante a integridade sob concorrência.
        builder.HasIndex(u => u.Nome)
            .IsUnique()
            .HasDatabaseName("UX_Usuario_Nome");

        builder.HasIndex(u => u.Ativo)
            .HasDatabaseName("IX_Usuario_Ativo");
    }
}
