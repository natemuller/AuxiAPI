using System.Collections.Generic;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;

namespace ServicesTest
{
    public class CondominioServiceTest
    {
        [Fact]
        public void ListarCondominios_QuandoConsultaVazia_DeveRetornarTodosOsCondominios()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery());

            Assert.Equal(2, resultado.Count);
            Assert.Equal("0001", resultado[0].CodigoDoCondominio);
            Assert.Equal("12345678000199", resultado[0].CNPJDoCondominio);
            Assert.Equal("01234567", resultado[0].CEPDoEndereco);
        }

        [Fact]
        public void ListarCondominios_QuandoFiltroPorCodigo_ComportamentoAtual_DeveNaoEncontrarCondominio()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery { CodigoDoCondominio = "1" });

            Assert.Empty(resultado);
        }

        [Fact]
        public void ListarCondominios_QuandoFiltroPorNome_DeveRetornarSomenteCondominioCorrespondente()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery { NomeDoCondominio = "sol" });

            var condominio = Assert.Single(resultado);
            Assert.Equal("Condomínio Sol Nascente", condominio.NomeDoCondominio);
            Assert.Equal("0010", condominio.CodigoDoCondominio);
        }

        [Fact]
        public void ListarCondominios_QuandoFiltroPorCnpjENome_DeveAplicarOsFiltrosEMapearDados()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery
            {
                CNPJDoCondominio = "12.345.678/0001-99",
                NomeDoCondominio = "aurora"
            });

            var condominio = Assert.Single(resultado);
            Assert.Equal("12345678000199", condominio.CNPJDoCondominio);
            Assert.Equal("01234567", condominio.CEPDoEndereco);
            Assert.Equal("Residencial Aurora", condominio.NomeDoCondominio);
        }

        [Fact]
        public void ListarCondominios_QuandoNenhumFiltroCombina_DeveRetornarListaVazia()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery { NomeDoCondominio = "inexistente" });

            Assert.Empty(resultado);
        }

        [Fact]
        public void ListarCondominios_QuandoFiltrosForemNulosOuVazios_DeveIgnorarFiltrosEListarTodos()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery
            {
                CodigoDoCondominio = null,
                CNPJDoCondominio = null,
                NomeDoCondominio = "   "
            });

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, c => c.NomeDoCondominio == "Residencial Aurora");
            Assert.Contains(resultado, c => c.NomeDoCondominio == "Condomínio Sol Nascente");
        }

        [Fact]
        public void ListarCondominios_QuandoCnpjTemCaracteresEspeciais_DeveNormalizarNaComparacaoEMapeamento()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery { CNPJDoCondominio = " 12.345.678/0001-99 " });

            var condominio = Assert.Single(resultado);
            Assert.Equal("12345678000199", condominio.CNPJDoCondominio);
            Assert.Equal("Residencial Aurora", condominio.NomeDoCondominio);
        }

        [Fact]
        public void ListarCondominios_QuandoNomeForMaiusculo_DeveEncontrarPorBuscaCaseInsensitive()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery { NomeDoCondominio = "AURORA" });

            var condominio = Assert.Single(resultado);
            Assert.Equal("Residencial Aurora", condominio.NomeDoCondominio);
        }

        [Fact]
        public void ListarCondominios_QuandoRepositorioEstiverVazio_DeveRetornarListaVazia()
        {
            var service = new CondominioService(new FakeCondominioRepository(new List<Condominio>()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery());

            Assert.Empty(resultado);
        }

        [Fact]
        public void ListarCondominios_QuandoCepForInformadoComMascara_DeveNormalizarNaSaida()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery { NomeDoCondominio = "aurora" });

            var condominio = Assert.Single(resultado);
            Assert.Equal("01234567", condominio.CEPDoEndereco);
        }

        [Fact]
        public void ListarCondominios_QuandoCodigoForInformadoComMenosDigitos_DeveManterComportamentoAtual()
        {
            var service = new CondominioService(new FakeCondominioRepository(CriarCondominios()));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery { CodigoDoCondominio = "1" });

            Assert.Empty(resultado);
        }

        [Fact]
        public void ListarCondominios_QuandoDadosDoCondominioForemParcialmenteNulos_DeveMapearSemErro()
        {
            var service = new CondominioService(new FakeCondominioRepository(new List<Condominio>
            {
                new()
                {
                    CodigoDoCondominio = "2",
                    NomeDoCondominio = "Residencial Teste"
                }
            }));

            var resultado = service.ListarCondominios(new VisualizarCondominioQuery());

            var condominio = Assert.Single(resultado);
            Assert.Equal("0002", condominio.CodigoDoCondominio);
            Assert.Equal(string.Empty, condominio.CNPJDoCondominio);
            Assert.Equal(string.Empty, condominio.CEPDoEndereco);
            Assert.Equal("Residencial Teste", condominio.NomeDoCondominio);
        }

        private static List<Condominio> CriarCondominios()
        {
            return new List<Condominio>
            {
                new()
                {
                    CodigoDoCondominio = "1",
                    CNPJDoCondominio = "12.345.678/0001-99",
                    NomeDoCondominio = "Residencial Aurora",
                    Endereco = "Rua das Flores",
                    NumeroDoEndereco = "100",
                    EstadoDoEndereco = "SP",
                    CidadeDoEndereco = "São Paulo",
                    BairroDoEndereco = "Centro",
                    CEPDoEndereco = "01234-567",
                    NumeroDeTorres = 2,
                    NumeroDeUnidades = 20,
                    Status = "Ativo",
                    DataInicial_Administracao = "2024-01-01",
                    DataFinal_Administracao = "2024-12-31",
                    NomeGerenteDeContas = "Maria Silva",
                    NomeSindico = "João Pereira"
                },
                new()
                {
                    CodigoDoCondominio = "10",
                    CNPJDoCondominio = "98.765.432/0001-10",
                    NomeDoCondominio = "Condomínio Sol Nascente",
                    Endereco = "Avenida Brasil",
                    NumeroDoEndereco = "250",
                    EstadoDoEndereco = "RJ",
                    CidadeDoEndereco = "Rio de Janeiro",
                    BairroDoEndereco = "Lapa",
                    CEPDoEndereco = "20000-000",
                    NumeroDeTorres = 4,
                    NumeroDeUnidades = 40,
                    Status = "Inativo",
                    DataInicial_Administracao = "2023-01-01",
                    DataFinal_Administracao = "2023-12-31",
                    NomeGerenteDeContas = "Ana Costa",
                    NomeSindico = "Carlos Mendes"
                }
            };
        }

        private sealed class FakeCondominioRepository(List<Condominio> condominios) : ICondominioRepository
        {
            public List<Condominio> LerJson() => condominios;
        }
    }
}