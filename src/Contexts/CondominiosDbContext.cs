using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.src.Contexts
{
    public class CondominiosDbContext(DbContextOptions<CondominiosDbContext> options) : DbContext(options)
    {
        public DbSet<Condominio> Condominios { get; set; }

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

            modelBuilder.Entity<Condominio>().HasData(
                new Condominio 
                { 
                    Id = 1,
                    CodigoDoCondominio = "0001", 
                    CNPJDoCondominio = "12345678000101", 
                    NomeDoCondominio = "Residencial Brasil-Hexa", 
                    Endereco = "Rua do Cambraia", 
                    NumeroDoEndereco = "5010", 
                    EstadoDoEndereco = "RS", 
                    CidadeDoEndereco = "Porto Alegre", 
                    BairroDoEndereco = "Auxiliadora", 
                    CEPDoEndereco = "98567213", 
                    NumeroDeTorres = 3, 
                    NumeroDeUnidades = 90, 
                    Status = "Ativo", 
                    DataInicial_Administracao = "20/08/2021", 
                    DataFinal_Administracao = "", 
                    NomeGerenteDeContas = "Antonio Banderas", 
                    NomeSindico = "Alfonso Herrera" 
                },
                new Condominio 
                { 
                    Id = 2,
                    CodigoDoCondominio = "0002", 
                    CNPJDoCondominio = "12543867000101", 
                    NomeDoCondominio = "Residencial Vivendas do Pelé", 
                    Endereco = "Rua do Marcel", 
                    NumeroDoEndereco = "103", 
                    EstadoDoEndereco = "RS", 
                    CidadeDoEndereco = "Porto Alegre", 
                    BairroDoEndereco = "Tristeza", 
                    CEPDoEndereco = "91183730",
                    NumeroDeTorres = 2, 
                    NumeroDeUnidades = 60, 
                    Status = "Inativo", 
                    DataInicial_Administracao = "10/04/2019", 
                    DataFinal_Administracao = "01/06/2026", 
                    NomeGerenteDeContas = "Christopher Uckermann", 
                    NomeSindico = "Lucio Flavio" 
                },
                new Condominio 
                { 
                    Id = 3,
                    CodigoDoCondominio = "0003", 
                    CNPJDoCondominio = "98765432000110", 
                    NomeDoCondominio = "Residencial Jardim das Flores", 
                    Endereco = "Avenida das Orquídeas", 
                    NumeroDoEndereco = "1200", 
                    EstadoDoEndereco = "RS", 
                    CidadeDoEndereco = "Porto Alegre", 
                    BairroDoEndereco = "Moinhos de Vento", 
                    CEPDoEndereco = "90450200", 
                    NumeroDeTorres = 4, 
                    NumeroDeUnidades = 120, 
                    Status = "Ativo", 
                    DataInicial_Administracao = "15/01/2023", 
                    DataFinal_Administracao = "", 
                    NomeGerenteDeContas = "Mariana Santos", 
                    NomeSindico = "Pedro Almeida" 
                },
                new Condominio 
                { 
                    Id = 4,
                    CodigoDoCondominio = "0004", 
                    CNPJDoCondominio = "11223344000155", 
                    NomeDoCondominio = "Condomínio Bela Vista", 
                    Endereco = "Rua das Acácias", 
                    NumeroDoEndereco = "250", 
                    EstadoDoEndereco = "RS", 
                    CidadeDoEndereco = "Canoas", 
                    BairroDoEndereco = "Centro", 
                    CEPDoEndereco = "92700120", 
                    NumeroDeTorres = 2, 
                    NumeroDeUnidades = 48, 
                    Status = "Ativo", 
                    DataInicial_Administracao = "05/09/2020", 
                    DataFinal_Administracao = "", 
                    NomeGerenteDeContas = "Yur Teixeira", 
                    NomeSindico = "Clara Mendes" 
                },
                new Condominio 
                { 
                    Id = 5,
                    CodigoDoCondominio = "0005", 
                    CNPJDoCondominio = "55667788000199", 
                    NomeDoCondominio = "Parque dos Pinheiros", 
                    Endereco = "Rua dos Eucaliptos", 
                    NumeroDoEndereco = "89", 
                    EstadoDoEndereco = "RS", 
                    CidadeDoEndereco = "São Leopoldo", 
                    BairroDoEndereco = "Santo Antônio", 
                    CEPDoEndereco = "93010300", 
                    NumeroDeTorres = 3, 
                    NumeroDeUnidades = 72, 
                    Status = "Inativo", 
                    DataInicial_Administracao = "22/11/2018", 
                    DataFinal_Administracao = "20/02/2024", 
                    NomeGerenteDeContas = "Juliana Pereira", 
                    NomeSindico = "Sharon Garcia" 
                },
                new Condominio 
                { 
                    Id = 6,
                    CodigoDoCondominio = "0006", 
                    CNPJDoCondominio = "22111333000166", 
                    NomeDoCondominio = "Condomínio Plaza Colucci", 
                    Endereco = "Avenida do Sol", 
                    NumeroDoEndereco = "144", 
                    EstadoDoEndereco = "SP", 
                    CidadeDoEndereco = "São Paulo", 
                    BairroDoEndereco = "Jardins", 
                    CEPDoEndereco = "01234567", 
                    NumeroDeTorres = 5, 
                    NumeroDeUnidades = 180, 
                    Status = "Ativo", 
                    DataInicial_Administracao = "01/03/2022", 
                    DataFinal_Administracao = "", 
                    NomeGerenteDeContas = "Mia Colucci", 
                    NomeSindico = "Miguel Arango" 
                },
                new Condominio 
                { 
                    Id = 7,
                    CodigoDoCondominio = "0007", 
                    CNPJDoCondominio = "33222444000177", 
                    NomeDoCondominio = "Residencial RBD", 
                    Endereco = "Rua da Harmonia", 
                    NumeroDoEndereco = "200", 
                    EstadoDoEndereco = "RJ", 
                    CidadeDoEndereco = "Rio de Janeiro", 
                    BairroDoEndereco = "Barra da Tijuca", 
                    CEPDoEndereco = "22345678", 
                    NumeroDeTorres = 4, 
                    NumeroDeUnidades = 120, 
                    Status = "Ativo", 
                    DataInicial_Administracao = "10/06/2023", 
                    DataFinal_Administracao = "", 
                    NomeGerenteDeContas = "Roberta Pardo", 
                    NomeSindico = "Diego Bustamante" 
                },
                new Condominio 
                { 
                    Id = 8,
                    CodigoDoCondominio = "0008", 
                    CNPJDoCondominio = "44333555000188", 
                    NomeDoCondominio = "Parque Celina", 
                    Endereco = "Travessa das Estrelas", 
                    NumeroDoEndereco = "77", 
                    EstadoDoEndereco = "MG", 
                    CidadeDoEndereco = "Belo Horizonte", 
                    BairroDoEndereco = "Savassi", 
                    CEPDoEndereco = "30456789", 
                    NumeroDeTorres = 3, 
                    NumeroDeUnidades = 90, 
                    Status = "Ativo", 
                    DataInicial_Administracao = "15/09/2021", 
                    DataFinal_Administracao = "", 
                    NomeGerenteDeContas = "Celina Ferrer", 
                    NomeSindico = "Lupita Fernández" 
                },
                new Condominio 
                { 
                    Id = 9,
                    CodigoDoCondominio = "0009", 
                    CNPJDoCondominio = "55444666000199", 
                    NomeDoCondominio = "Vista Altos", 
                    Endereco = "Boulevard das Artes", 
                    NumeroDoEndereco = "310", 
                    EstadoDoEndereco = "CE", 
                    CidadeDoEndereco = "Fortaleza", 
                    BairroDoEndereco = "Meireles", 
                    CEPDoEndereco = "60123456", 
                    NumeroDeTorres = 6, 
                    NumeroDeUnidades = 240, 
                    Status = "Ativo", 
                    DataInicial_Administracao = "20/02/2024", 
                    DataFinal_Administracao = "", 
                    NomeGerenteDeContas = "Josy Luján", 
                    NomeSindico = "Giovanni Méndez" 
                },
                new Condominio 
                { 
                    Id = 10,
                    CodigoDoCondominio = "0010", 
                    CNPJDoCondominio = "66555777000111", 
                    NomeDoCondominio = "Condomínio Esperança", 
                    Endereco = "Rua dos Sonhos", 
                    NumeroDoEndereco = "510", 
                    EstadoDoEndereco = "SP", 
                    CidadeDoEndereco = "Campinas", 
                    BairroDoEndereco = "Cambuí", 
                    CEPDoEndereco = "13234567", 
                    NumeroDeTorres = 2, 
                    NumeroDeUnidades = 64, 
                    Status = "Ativo", 
                    DataInicial_Administracao = "05/11/2022", 
                    DataFinal_Administracao = "", 
                    NomeGerenteDeContas = "Victoria Díaz", 
                    NomeSindico = "Lupita Fernández" 
                }
            );
            base.OnModelCreating(modelBuilder);
        }
    }
}