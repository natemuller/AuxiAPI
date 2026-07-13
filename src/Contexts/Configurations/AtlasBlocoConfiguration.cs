using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuxiAPI.src.Contexts.Configurations;

public class AtlasBlocoConfiguration : IEntityTypeConfiguration<AtlasBloco>
{
    public void Configure(EntityTypeBuilder<AtlasBloco> builder)
    {
        builder.ToTable("atlas_blocos");

        builder.HasKey(b => new
        {
            b.CodCondom,
            b.CodBloco
        });

        builder.Property(b => b.CodCondom)
            .HasColumnName("codcondom");

        builder.Property(b => b.CodBloco)
            .HasColumnName("codbloco");

        builder.Property(b => b.CodBlocoBase)
            .HasColumnName("codblocobase");

        builder.Property(b => b.Descricao)
            .HasColumnName("descricao");

        builder.Property(b => b.QtdEconomias)
            .HasColumnName("qtdeconomias");

        builder.Property(b => b.TipoLograd)
            .HasColumnName("tipolograd");

        builder.Property(b => b.Lograd)
            .HasColumnName("lograd");

        builder.Property(b => b.Numero)
            .HasColumnName("numero");

        builder.Property(b => b.Bairro)
            .HasColumnName("bairro");

        builder.Property(b => b.Cidade)
            .HasColumnName("cidade");

        builder.Property(b => b.Uf)
            .HasColumnName("uf");

        builder.Property(b => b.Cep8Log)
            .HasColumnName("cep8_log");

        builder.Property(b => b.Ativo)
            .HasColumnName("ativo");

        builder.Property(b => b.TipoBloco)
            .HasColumnName("tipo_bloco");

        builder.HasOne(b => b.Condominio)
            .WithMany(c => c.Blocos)
            .HasForeignKey(b => b.CodCondom)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Unidades)
            .WithOne(u => u.Bloco)
            .HasForeignKey(u => new
            {
                u.CodCondom,
                u.CodBloco
            })
            .HasPrincipalKey(b => new
            {
                b.CodCondom,
                b.CodBloco
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}