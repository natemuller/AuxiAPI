using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
using Xunit;

namespace AuxiAPI.Test.ServicesTest
{
    public class UnidadeServiceTest
    {
        [Fact]
        public async Task ListarUnidadesAsync_QuandoNaoHouverFiltro_DeveRetornarResultadoPaginado()
        {
            var repository = new UnidadeRepositoryFake
            {
                Itens = new List<AtlasUnidade>
                {
                    CriarUnidade(1, 5396, "João Silva"),
                    CriarUnidade(2, 5396, "Maria Souza")
                },
                TotalItens = 2
            };

            var cacheService = new DatabaseCacheServiceFake();

            var service = new UnidadeService(repository, cacheService);

            var query = new VisualizarUnidadeQuery();

            var resultado = await service.ListarUnidadesAsync(query);

            Assert.Equal(1, resultado.Pagina);
            Assert.Equal(10, resultado.TamanhoPagina);
            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(1, resultado.TotalPaginas);
            Assert.Equal(2, resultado.Itens.Count);

            Assert.Equal(1, resultado.Itens[0].IdEconomia);
            Assert.Equal(5396, resultado.Itens[0].CodCondom);
            Assert.Equal("João Silva", resultado.Itens[0].NomeCondomino);
        }

        [Fact]
        public async Task ListarUnidadesAsync_QuandoPaginaForZero_DeveLancarArgumentException()
        {
            var service = new UnidadeService(
                new UnidadeRepositoryFake(),
                new DatabaseCacheServiceFake());

            var query = new VisualizarUnidadeQuery
            {
                Pagina = 0
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ListarUnidadesAsync(query));
        }

        [Fact]
        public async Task ListarUnidadesAsync_QuandoCodCondomForInvalido_DeveLancarArgumentException()
        {
            var service = new UnidadeService(
                new UnidadeRepositoryFake(),
                new DatabaseCacheServiceFake());

            var query = new VisualizarUnidadeQuery
            {
                CodCondom = 0
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ListarUnidadesAsync(query));
        }

        [Fact]
        public async Task ListarUnidadesAsync_QuandoNomeCondominoPassarDe200Caracteres_DeveLancarArgumentException()
        {
            var service = new UnidadeService(
                new UnidadeRepositoryFake(),
                new DatabaseCacheServiceFake());

            var query = new VisualizarUnidadeQuery
            {
                NomeCondomino = new string('a', 201)
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ListarUnidadesAsync(query));
        }

        [Fact]
        public async Task ObterPorIdEconomiaAsync_QuandoUnidadeExistir_DeveRetornarDto()
        {
            var unidade = CriarUnidade(123, 5396, "João Silva");

            var repository = new UnidadeRepositoryFake
            {
                UnidadePorIdEconomia = unidade
            };

            var cacheService = new DatabaseCacheServiceFake();

            var service = new UnidadeService(repository, cacheService);

            var resultado = await service.ObterPorIdEconomiaAsync(123);

            Assert.Equal(123, resultado.IdEconomia);
            Assert.Equal(5396, resultado.CodCondom);
            Assert.Equal("João Silva", resultado.NomeCondomino);
            Assert.Equal("45000", resultado.DataDesativa);
        }

        [Fact]
        public async Task ObterPorIdEconomiaAsync_QuandoUnidadeNaoExistir_DeveLancarKeyNotFoundException()
        {
            var repository = new UnidadeRepositoryFake
            {
                UnidadePorIdEconomia = null
            };

            var cacheService = new DatabaseCacheServiceFake();

            var service = new UnidadeService(repository, cacheService);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.ObterPorIdEconomiaAsync(999999));
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