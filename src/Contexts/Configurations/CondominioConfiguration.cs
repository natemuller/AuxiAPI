using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuxiAPI.src.Contexts.Configurations;

public class CondominioConfiguration : IEntityTypeConfiguration<Condominio>
{
    public void Configure(EntityTypeBuilder<Condominio> builder)
    {
        builder.ToTable("condominios");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.CodigoDoCondominio)
            .HasColumnName("codigoDoCondominio");

        builder.Property(c => c.CNPJDoCondominio)
            .HasColumnName("cnpjDoCondominio");

        builder.Property(c => c.NomeDoCondominio)
            .HasColumnName("nomeDoCondominio");

        builder.Property(c => c.Endereco)
            .HasColumnName("endereco");

        builder.Property(c => c.NumeroDoEndereco)
            .HasColumnName("numeroDoEndereco");

        builder.Property(c => c.EstadoDoEndereco)
            .HasColumnName("estadoDoEndereco");

        builder.Property(c => c.CidadeDoEndereco)
            .HasColumnName("cidadeDoEndereco");

        builder.Property(c => c.BairroDoEndereco)
            .HasColumnName("bairroDoEndereco");

        builder.Property(c => c.CEPDoEndereco)
            .HasColumnName("cepDoEndereco");

        builder.Property(c => c.NumeroDeTorres)
            .HasColumnName("numeroDeTorres");

        builder.Property(c => c.NumeroDeUnidades)
            .HasColumnName("numeroDeUnidades");

        builder.Property(c => c.Status)
            .HasColumnName("status");

        builder.Property(c => c.DataInicial_Administracao)
            .HasColumnName("dataInicial_Administracao");

        builder.Property(c => c.DataFinal_Administracao)
            .HasColumnName("dataFinal_Administracao");

        builder.Property(c => c.NomeGerenteDeContas)
            .HasColumnName("nomeGerenteDeContas");

        builder.Property(c => c.NomeSindico)
            .HasColumnName("nomeSindico");
    }
}