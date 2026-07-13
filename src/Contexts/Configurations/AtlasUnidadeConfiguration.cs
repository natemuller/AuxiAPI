using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuxiAPI.src.Contexts.Configurations;

public class AtlasUnidadeConfiguration : IEntityTypeConfiguration<AtlasUnidade>
{
    public void Configure(EntityTypeBuilder<AtlasUnidade> builder)
    {
        builder.ToTable("atlas_unidades");

        builder.HasKey(u => u.IdEconomia);

        builder.Property(u => u.IdEconomia)
            .HasColumnName("ideconomia");

        builder.Property(u => u.CodCondom)
            .HasColumnName("codcondom");

        builder.Property(u => u.CodBloco)
            .HasColumnName("codbloco");

        builder.Property(u => u.CodEconom)
            .HasColumnName("codeconom");

        builder.Property(u => u.Fracao)
            .HasColumnName("fracao")
            .HasColumnType("numeric(18, 8)");

        builder.Property(u => u.Ativa)
            .HasColumnName("ativa");

        builder.Property(u => u.DataDesativa)
            .HasColumnName("datadesativa")
            .HasColumnType("numeric(18, 2)");

        builder.Property(u => u.DtAlteracao)
            .HasColumnName("dtalteracao");

        builder.Property(u => u.TipoUnidade)
            .HasColumnName("tipo_unidade");

        builder.Property(u => u.CodCondomino)
            .HasColumnName("cod_condomino");

        builder.Property(u => u.NomeCondomino)
            .HasColumnName("nome_condomino");

        builder.Property(u => u.EnderecoPrincipal)
            .HasColumnName("endereco_principal");

        builder.Property(u => u.EnderecoCorrespondencia)
            .HasColumnName("endereco_correspondencia");

        builder.Property(u => u.EnderecoCobranca)
            .HasColumnName("endereco_cobranca");

        builder.Property(u => u.CodPesDebConta)
            .HasColumnName("codpesdebconta");

        builder.Property(u => u.NomeDebConta)
            .HasColumnName("nome_debconta");

        builder.Property(u => u.CodFornec)
            .HasColumnName("codfornec");

        builder.Property(u => u.CodNaAdmDest)
            .HasColumnName("codnaadmdest");

        builder.Property(u => u.CodFornecEscrit)
            .HasColumnName("codfornecescrit");

        builder.HasOne(u => u.Condominio)
            .WithMany(c => c.Unidades)
            .HasForeignKey(u => u.CodCondom)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Bloco)
            .WithMany(b => b.Unidades)
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