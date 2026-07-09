using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuxiAPI.src.Contexts.Configurations;

public class CacheEntryConfiguration : IEntityTypeConfiguration<CacheEntry>
{
    public void Configure(EntityTypeBuilder<CacheEntry> builder)
    {
        builder.ToTable("cache");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.ChaveCache)
            .HasColumnName("chave_cache")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.UrlDaConsulta)
            .HasColumnName("url_da_consulta")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.MetodoHttp)
            .HasColumnName("metodo_http")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.TipoConsulta)
            .HasColumnName("tipo_consulta")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Entidade)
            .HasColumnName("entidade")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.EntidadeId)
            .HasColumnName("entidade_id");

        builder.Property(c => c.Resposta)
            .HasColumnName("resposta")
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(c => c.StatusCode)
            .HasColumnName("status_code")
            .IsRequired();

        builder.Property(c => c.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(c => c.ExpiradoEm)
            .HasColumnName("expirado_em")
            .IsRequired();

        builder.Property(c => c.InvalidadoEm)
            .HasColumnName("invalidado_em");

        builder.Property(c => c.MotivoInvalidacao)
            .HasColumnName("motivo_invalidacao")
            .HasMaxLength(255);

        builder.HasIndex(c => c.ChaveCache);

        builder.HasIndex(c => new
        {
            c.ChaveCache,
            c.ExpiradoEm,
            c.InvalidadoEm
        });

        builder.HasIndex(c => new
        {
            c.Entidade,
            c.EntidadeId
        });

        builder.HasIndex(c => c.TipoConsulta);
    }
}