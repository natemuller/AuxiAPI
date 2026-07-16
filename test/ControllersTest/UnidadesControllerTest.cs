using AuxiAPI.src.Controllers;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AuxiAPI.Test.ControllersTest
{
    public class UnidadesControllerTest
    {
        [Fact]
        public async Task GetAll_DeveRetornarOkComResultadoPaginado()
        {
            var repository = new UnidadeRepositoryFake
            {
                Itens =
                [
                    CriarUnidade(123, 5396, "João Silva"),
                    CriarUnidade(124, 5396, "Maria Souza")
                ],
                TotalItens = 2
            };

            var service = new UnidadeService(
                repository,
                new DatabaseCacheServiceFake());

            var controller = new UnidadesController(service);

            var query = new VisualizarUnidadeQuery
            {
                CodCondom = 5396,
                Pagina = 1
            };

            var resposta = await controller.GetAll(query);

            var okResult = Assert.IsType<OkObjectResult>(resposta);
            var resultado = Assert.IsType<ResultadoPaginadoDto<AtlasUnidadeDto>>(okResult.Value);

            Assert.Equal(1, resultado.Pagina);
            Assert.Equal(10, resultado.TamanhoPagina);
            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(1, resultado.TotalPaginas);
            Assert.Equal(2, resultado.Itens.Count);
            Assert.Equal(5396, resultado.Itens[0].CodCondom);
        }

        [Fact]
        public async Task GetAll_QuandoQueryPorNomeCondomino_DeveRetornarOk()
        {
            var repository = new UnidadeRepositoryFake
            {
                Itens =
                [
                    CriarUnidade(123, 5396, "João Silva")
                ],
                TotalItens = 1
            };

            var service = new UnidadeService(
                repository,
                new DatabaseCacheServiceFake());

            var controller = new UnidadesController(service);

            var query = new VisualizarUnidadeQuery
            {
                NomeCondomino = "João",
                Pagina = 1
            };

            var resposta = await controller.GetAll(query);

            var okResult = Assert.IsType<OkObjectResult>(resposta);
            var resultado = Assert.IsType<ResultadoPaginadoDto<AtlasUnidadeDto>>(okResult.Value);

            Assert.Single(resultado.Itens);
            Assert.Equal("João Silva", resultado.Itens[0].NomeCondomino);
        }

        [Fact]
        public async Task GetByIdEconomia_QuandoUnidadeExistir_DeveRetornarOkComUnidade()
        {
            var repository = new UnidadeRepositoryFake
            {
                UnidadePorIdEconomia = CriarUnidade(123, 5396, "João Silva")
            };

            var service = new UnidadeService(
                repository,
                new DatabaseCacheServiceFake());

            var controller = new UnidadesController(service);

            var resposta = await controller.GetByIdEconomia(123);

            var okResult = Assert.IsType<OkObjectResult>(resposta);
            var resultado = Assert.IsType<AtlasUnidadeDto>(okResult.Value);

            Assert.Equal(123, resultado.IdEconomia);
            Assert.Equal(5396, resultado.CodCondom);
            Assert.Equal("João Silva", resultado.NomeCondomino);
        }

        [Fact]
        public async Task GetByIdEconomia_QuandoUnidadeNaoExistir_DeveLancarKeyNotFoundException()
        {
            var repository = new UnidadeRepositoryFake
            {
                UnidadePorIdEconomia = null
            };

            var service = new UnidadeService(
                repository,
                new DatabaseCacheServiceFake());

            var controller = new UnidadesController(service);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                controller.GetByIdEconomia(999999));
        }

        [Fact]
        public async Task GetAll_QuandoPaginaForInvalida_DeveLancarArgumentException()
        {
            var service = new UnidadeService(
                new UnidadeRepositoryFake(),
                new DatabaseCacheServiceFake());

            var controller = new UnidadesController(service);

            var query = new VisualizarUnidadeQuery
            {
                Pagina = 0
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                controller.GetAll(query));
        }

        private static AtlasUnidade CriarUnidade(
            int idEconomia,
            int codCondom,
            string nomeCondomino)
        {
            return new AtlasUnidade
            {
                IdEconomia = idEconomia,
                CodCondom = codCondom,
                CodBloco = "A",
                CodEconom = "101",
                Fracao = 0.08280000m,
                Ativa = "S",
                DataDesativa = 45000m,
                DtAlteracao = new DateTime(2026, 7, 16),
                TipoUnidade = "Apartamento",
                CodCondomino = "999",
                NomeCondomino = nomeCondomino,
                EnderecoPrincipal = "Rua Teste, 123"
            };
        }

        private class UnidadeRepositoryFake : IUnidadeRepository
        {
            public List<AtlasUnidade> Itens { get; set; } = [];
            public int TotalItens { get; set; }
            public AtlasUnidade? UnidadePorIdEconomia { get; set; }

            public Task<(List<AtlasUnidade> Itens, int TotalItens)> ListarAsync(
                VisualizarUnidadeQuery query,
                int tamanhoPagina)
            {
                return Task.FromResult((Itens, TotalItens));
            }

            public Task<AtlasUnidade?> ObterPorIdEconomiaAsync(int idEconomia)
            {
                return Task.FromResult(UnidadePorIdEconomia);
            }
        }

        private class DatabaseCacheServiceFake : IDatabaseCacheService
        {
            public Task<T?> ObterAsync<T>(string chaveCache) where T : class
            {
                return Task.FromResult<T?>(null);
            }

            public Task SalvarAsync<T>(
                string chaveCache,
                string urlDaConsulta,
                string tipoConsulta,
                string entidade,
                int? entidadeId,
                T resposta,
                int statusCode = 200) where T : class
            {
                return Task.CompletedTask;
            }
        }
    }
}