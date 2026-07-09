using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.src.Contexts
{
    public class CondominiosDbContext(DbContextOptions<CondominiosDbContext> options) : DbContext(options)
    {
        public DbSet<Condominio> Condominios { get; set; }
        public DbSet<CacheEntry> Cache {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Condominio>().ToTable("condominios");

            modelBuilder.Entity<Condominio>().HasKey(c => c.Id);
            modelBuilder.Entity<Condominio>().Property(c => c.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Condominio>().Property(c => c.CodigoDoCondominio).HasColumnName("codigoDoCondominio");
            modelBuilder.Entity<Condominio>().Property(c => c.CNPJDoCondominio).HasColumnName("cnpjDoCondominio");
            modelBuilder.Entity<Condominio>().Property(c => c.NomeDoCondominio).HasColumnName("nomeDoCondominio");
            modelBuilder.Entity<Condominio>().Property(c => c.Endereco).HasColumnName("endereco");
            modelBuilder.Entity<Condominio>().Property(c => c.NumeroDoEndereco).HasColumnName("numeroDoEndereco");
            modelBuilder.Entity<Condominio>().Property(c => c.EstadoDoEndereco).HasColumnName("estadoDoEndereco");
            modelBuilder.Entity<Condominio>().Property(c => c.CidadeDoEndereco).HasColumnName("cidadeDoEndereco");
            modelBuilder.Entity<Condominio>().Property(c => c.BairroDoEndereco).HasColumnName("bairroDoEndereco");
            modelBuilder.Entity<Condominio>().Property(c => c.CEPDoEndereco).HasColumnName("cepDoEndereco");
            modelBuilder.Entity<Condominio>().Property(c => c.NumeroDeTorres).HasColumnName("numeroDeTorres");
            modelBuilder.Entity<Condominio>().Property(c => c.NumeroDeUnidades).HasColumnName("numeroDeUnidades");
            modelBuilder.Entity<Condominio>().Property(c => c.Status).HasColumnName("status");
            modelBuilder.Entity<Condominio>().Property(c => c.DataInicial_Administracao).HasColumnName("dataInicial_Administracao");
            modelBuilder.Entity<Condominio>().Property(c => c.DataFinal_Administracao).HasColumnName("dataFinal_Administracao");
            modelBuilder.Entity<Condominio>().Property(c => c.NomeGerenteDeContas).HasColumnName("nomeGerenteDeContas");
            modelBuilder.Entity<Condominio>().Property(c => c.NomeSindico).HasColumnName("nomeSindico");

            modelBuilder.Entity<CacheEntry>().ToTable("cache");

            modelBuilder.Entity<CacheEntry>().HasKey(c => c.Id);
            modelBuilder.Entity<CacheEntry>().Property(c => c.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CacheEntry>().Property(c => c.ChaveCache)
                .HasColumnName("chave_cache")
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<CacheEntry>().Property(c => c.UrlDaConsulta)
                .HasColumnName("url_da_consulta")
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<CacheEntry>().Property(c => c.MetodoHttp)
                .HasColumnName("metodo_http")
                .IsRequired()
                .HasMaxLength(10);

            modelBuilder.Entity<CacheEntry>().Property(c => c.TipoConsulta)
                .HasColumnName("tipo_consulta")
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<CacheEntry>().Property(c => c.Entidade)
                .HasColumnName("entidade")
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<CacheEntry>().Property(c => c.EntidadeId)
                .HasColumnName("entidade_id");

            modelBuilder.Entity<CacheEntry>().Property(c => c.Resposta)
                .HasColumnName("resposta")
                .IsRequired()
                .HasColumnType("jsonb");

            modelBuilder.Entity<CacheEntry>().Property(c => c.StatusCode)
                .HasColumnName("status_code")
                .IsRequired();

            modelBuilder.Entity<CacheEntry>().Property(c => c.CriadoEm)
                .HasColumnName("criado_em")
                .IsRequired();

            modelBuilder.Entity<CacheEntry>().Property(c => c.ExpiradoEm)
                .HasColumnName("expirado_em")
                .IsRequired();

            modelBuilder.Entity<CacheEntry>().Property(c => c.InvalidadoEm)
                .HasColumnName("invalidado_em");

            modelBuilder.Entity<CacheEntry>().Property(c => c.MotivoInvalidacao)
                .HasColumnName("motivo_invalidacao")
                .HasMaxLength(255);

            modelBuilder.Entity<CacheEntry>()
                .HasIndex(c => c.ChaveCache);

            modelBuilder.Entity<CacheEntry>()
                .HasIndex(c => new { c.ChaveCache, c.ExpiradoEm, c.InvalidadoEm });

            modelBuilder.Entity<CacheEntry>()
                .HasIndex(c => new { c.Entidade, c.EntidadeId });

            modelBuilder.Entity<CacheEntry>()
                .HasIndex(c => c.TipoConsulta);

            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasDbFunction(typeof(PostgresDbFunctions)
                    .GetMethod(nameof(PostgresDbFunctions.Unaccent), new[] { typeof(string) })!)
                .HasName("unaccent")
                .HasSchema("public");
        }
    }
}