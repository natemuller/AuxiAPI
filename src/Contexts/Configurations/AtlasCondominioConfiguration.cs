using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuxiAPI.src.Contexts.Configurations;

public class AtlasCondominioConfiguration : IEntityTypeConfiguration<AtlasCondominio>
{
    public void Configure(EntityTypeBuilder<AtlasCondominio> builder)
    {
        builder.ToTable("atlas_condominios");

        builder.HasKey(c => c.CodCondom);

        builder.Property(c => c.CodCondom)
            .HasColumnName("codcondom");

        builder.Property(c => c.NomeCondom)
            .HasColumnName("nomecondom");

        builder.Property(c => c.Ativo)
            .HasColumnName("ativo");

        builder.Property(c => c.Cnpj)
            .HasColumnName("cnpj");

        builder.Property(c => c.Cei)
            .HasColumnName("cei");

        builder.Property(c => c.InscrMunicip)
            .HasColumnName("inscrmunicip");

        builder.Property(c => c.QtdBlocos)
            .HasColumnName("qtdblocos");

        builder.Property(c => c.QtdUnidades)
            .HasColumnName("qtd_unidades");

        builder.Property(c => c.TotalFracao)
            .HasColumnName("totalfracao")
            .HasColumnType("numeric(28, 8)");

        builder.Property(c => c.DiaVencDoc)
            .HasColumnName("diavencdoc");

        builder.Property(c => c.DataInicioAdm)
            .HasColumnName("datainicioadm");

        builder.Property(c => c.DataDistrato)
            .HasColumnName("datadistrato");

        builder.Property(c => c.MotivoDistrato)
            .HasColumnName("motivodistrato");

        builder.Property(c => c.Assessor)
            .HasColumnName("assessor");

        builder.Property(c => c.Filial)
            .HasColumnName("filial");

        builder.Property(c => c.Agencia)
            .HasColumnName("agencia");

        builder.Property(c => c.Sindico)
            .HasColumnName("sindico");

        builder.Property(c => c.SubSindico)
            .HasColumnName("subsindico");

        builder.Property(c => c.Conselheiro)
            .HasColumnName("conselheiro");

        builder.Property(c => c.Gestor)
            .HasColumnName("gestor");

        builder.Property(c => c.ConselhoFiscal)
            .HasColumnName("conselho_fiscal");

        builder.Property(c => c.ConselhoConsultivo)
            .HasColumnName("conselho_consultivo");

        builder.Property(c => c.ConselhoSuplente)
            .HasColumnName("conselho_suplente");

        builder.Property(c => c.TipoCondominio)
            .HasColumnName("tipocondominio");

        builder.Property(c => c.TipoCategoria)
            .HasColumnName("tipocategoria");

        builder.Property(c => c.DtAlteracao)
            .HasColumnName("dtalteracao")
            .HasColumnType("timestamp without time zone");

        builder.Property(c => c.TipoLograd)
            .HasColumnName("tipolograd");

        builder.Property(c => c.Lograd)
            .HasColumnName("lograd");

        builder.Property(c => c.Numero)
            .HasColumnName("numero");

        builder.Property(c => c.Bairro)
            .HasColumnName("bairro");

        builder.Property(c => c.Cidade)
            .HasColumnName("cidade");

        builder.Property(c => c.Cep8Log)
            .HasColumnName("cep8_log");

        builder.Property(c => c.Uf)
            .HasColumnName("uf");

        builder.Property(c => c.CodPessoaSindico)
            .HasColumnName("codpessoa_sindico");

        builder.Property(c => c.NomeSindico)
            .HasColumnName("nome_sindico");

        builder.Property(c => c.CpfDocnpj)
            .HasColumnName("cpfdocnpj");

        builder.Property(c => c.CondGarantido)
            .HasColumnName("condgarantido");

        builder.Property(c => c.TipoConta)
            .HasColumnName("tipoconta");

        builder.Property(c => c.ObsCobranca)
            .HasColumnName("obscobranca");

        builder.Property(c => c.Garantidora)
            .HasColumnName("garantidora");

        builder.HasMany(c => c.Blocos)
            .WithOne(b => b.Condominio)
            .HasForeignKey(b => b.CodCondom)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Unidades)
            .WithOne(u => u.Condominio)
            .HasForeignKey(u => u.CodCondom)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
