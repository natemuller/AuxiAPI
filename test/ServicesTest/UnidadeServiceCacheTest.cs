using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
using Xunit;

namespace AuxiAPI.Test.ServicesTest
{
    public class UnidadeServiceCacheTest
    {
        [Fact]
        public async Task ObterPorIdEconomiaAsync_QuandoCacheExistir_DeveRetornarCacheSemConsultarRepository()
        {
            var dtoCache = CriarDto(123, 5396, "João Silva");

            var cacheService = new DatabaseCacheServiceFake();
            cacheService.Cache["GET:/api/unidades/123"] = dtoCache;

            var repository = new UnidadeRepositoryFake();

            var service = new UnidadeService(repository, cacheService);

            var resultado = await service.ObterPorIdEconomiaAsync(123);

            Assert.Equal(123, resultado.IdEconomia);
            Assert.Equal("João Silva", resultado.NomeCondomino);
            Assert.Equal(0, repository.QuantidadeChamadasObterPorIdEconomia);
        }

        [Fact]
        public async Task ObterPorIdEconomiaAsync_QuandoCacheNaoExistir_DeveConsultarRepositoryESalvarCache()
        {
            var repository = new UnidadeRepositoryFake
            {
                UnidadePorIdEconomia = CriarUnidade(123, 5396, "João Silva")
            };

            var cacheService = new DatabaseCacheServiceFake();

            var service = new UnidadeService(repository, cacheService);

            var resultado = await service.ObterPorIdEconomiaAsync(123);

            Assert.Equal(123, resultado.IdEconomia);
            Assert.Equal(1, repository.QuantidadeChamadasObterPorIdEconomia);

            var chamada = Assert.Single(cacheService.ChamadasSalvar);

            Assert.Equal("GET:/api/unidades/123", chamada.ChaveCache);
            Assert.Equal("/api/unidades/123", chamada.UrlDaConsulta);
            Assert.Equal("UNIDADE_IDECONOMIA", chamada.TipoConsulta);
            Assert.Equal("atlas_unidades", chamada.Entidade);
            Assert.Equal(123, chamada.EntidadeId);
        }

        [Fact]
        public async Task ListarUnidadesAsync_QuandoCachePorCodCondomExistir_DeveRetornarCacheSemConsultarRepository()
        {
            var resultadoCache = new ResultadoPaginadoDto<AtlasUnidadeDto>
            {
                Pagina = 1,
                TamanhoPagina = 10,
                TotalItens = 1,
                TotalPaginas = 1,
                Itens =
                [
                    CriarDto(123, 5396, "João Silva")
                ]
            };

            var cacheService = new DatabaseCacheServiceFake();
            cacheService.Cache["GET:/api/unidades?codCondom=5396&pagina=1"] = resultadoCache;

            var repository = new UnidadeRepositoryFake();

            var service = new UnidadeService(repository, cacheService);

            var query = new VisualizarUnidadeQuery
            {
                CodCondom = 5396,
                Pagina = 1
            };

            var resultado = await service.ListarUnidadesAsync(query);

            Assert.Single(resultado.Itens);
            Assert.Equal(5396, resultado.Itens[0].CodCondom);
            Assert.Equal(0, repository.QuantidadeChamadasListar);
        }

        [Fact]
        public async Task ListarUnidadesAsync_QuandoFiltroSomenteCodCondom_DeveSalvarCache()
        {
            var repository = new UnidadeRepositoryFake
            {
                Itens =
                [
                    CriarUnidade(123, 5396, "João Silva")
                ],
                TotalItens = 1
            };

            var cacheService = new DatabaseCacheServiceFake();

            var service = new UnidadeService(repository, cacheService);

            var query = new VisualizarUnidadeQuery
            {
                CodCondom = 5396,
                Pagina = 1
            };

            var resultado = await service.ListarUnidadesAsync(query);

            Assert.Single(resultado.Itens);
            Assert.Equal(1, repository.QuantidadeChamadasListar);

            var chamada = Assert.Single(cacheService.ChamadasSalvar);

            Assert.Equal("GET:/api/unidades?codCondom=5396&pagina=1", chamada.ChaveCache);
            Assert.Equal("/api/unidades?codCondom=5396&pagina=1", chamada.UrlDaConsulta);
            Assert.Equal("UNIDADE_CODCONDOM", chamada.TipoConsulta);
            Assert.Equal("atlas_unidades", chamada.Entidade);
            Assert.Null(chamada.EntidadeId);
        }

        [Fact]
        public async Task ListarUnidadesAsync_QuandoFiltroSomenteNomeCondomino_DeveSalvarCacheComNomeNormalizado()
        {
            var repository = new UnidadeRepositoryFake
            {
                Itens =
                [
                    CriarUnidade(123, 5396, "João Silva")
                ],
                TotalItens = 1
            };

            var cacheService = new DatabaseCacheServiceFake();

            var service = new UnidadeService(repository, cacheService);

            var query = new VisualizarUnidadeQuery
            {
                NomeCondomino = "João",
                Pagina = 1
            };

            var resultado = await service.ListarUnidadesAsync(query);

            Assert.Single(resultado.Itens);
            Assert.Equal(1, repository.QuantidadeChamadasListar);

            var chamada = Assert.Single(cacheService.ChamadasSalvar);

            Assert.Equal("GET:/api/unidades?nomeCondomino=joao&pagina=1", chamada.ChaveCache);
            Assert.Equal("/api/unidades?nomeCondomino=joao&pagina=1", chamada.UrlDaConsulta);
            Assert.Equal("UNIDADE_NOME_CONDOMINO", chamada.TipoConsulta);
            Assert.Equal("atlas_unidades", chamada.Entidade);
            Assert.Null(chamada.EntidadeId);
        }

        [Fact]
        public async Task ListarUnidadesAsync_QuandoNaoHouverFiltro_NaoDeveSalvarCache()
        {
            var repository = new UnidadeRepositoryFake
            {
                Itens =
                [
                    CriarUnidade(123, 5396, "João Silva")
                ],
                TotalItens = 1
            };

            var cacheService = new DatabaseCacheServiceFake();

            var service = new UnidadeService(repository, cacheService);

            var query = new VisualizarUnidadeQuery();

            var resultado = await service.ListarUnidadesAsync(query);

            Assert.Single(resultado.Itens);
            Assert.Equal(1, repository.QuantidadeChamadasListar);
            Assert.Empty(cacheService.ChamadasSalvar);
        }

        [Fact]
        public async Task ListarUnidadesAsync_QuandoFiltroForCombinado_NaoDeveSalvarCache()
        {
            var repository = new UnidadeRepositoryFake
            {
                Itens =
                [
                    CriarUnidade(123, 5396, "João Silva")
                ],
                TotalItens = 1
            };

            var cacheService = new DatabaseCacheServiceFake();

            var service = new UnidadeService(repository, cacheService);

            var query = new VisualizarUnidadeQuery
            {
                CodCondom = 5396,
                NomeCondomino = "João",
                Pagina = 1
            };

            var resultado = await service.ListarUnidadesAsync(query);

            Assert.Single(resultado.Itens);
            Assert.Equal(1, repository.QuantidadeChamadasListar);
            Assert.Empty(cacheService.ChamadasSalvar);
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

        private static AtlasUnidadeDto CriarDto(
            int idEconomia,
            int codCondom,
            string nomeCondomino)
        {
            return new AtlasUnidadeDto
            {
                IdEconomia = idEconomia,
                CodCondom = codCondom,
                CodBloco = "A",
                CodEconom = "101",
                Fracao = 0.08280000m,
                Ativa = "S",
                DataDesativa = "45000",
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

            public int QuantidadeChamadasListar { get; private set; }
            public int QuantidadeChamadasObterPorIdEconomia { get; private set; }

            public Task<(List<AtlasUnidade> Itens, int TotalItens)> ListarAsync(
                VisualizarUnidadeQuery query,
                int tamanhoPagina)
            {
                QuantidadeChamadasListar++;
                return Task.FromResult((Itens, TotalItens));
            }

            public Task<AtlasUnidade?> ObterPorIdEconomiaAsync(int idEconomia)
            {
                QuantidadeChamadasObterPorIdEconomia++;
                return Task.FromResult(UnidadePorIdEconomia);
            }
        }

        private class DatabaseCacheServiceFake : IDatabaseCacheService
        {
            public Dictionary<string, object> Cache { get; } = [];
            public List<ChamadaSalvarCache> ChamadasSalvar { get; } = [];

            public Task<T?> ObterAsync<T>(string chaveCache) where T : class
            {
                if (Cache.TryGetValue(chaveCache, out var valor))
                    return Task.FromResult(valor as T);

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
                ChamadasSalvar.Add(new ChamadaSalvarCache
                {
                    ChaveCache = chaveCache,
                    UrlDaConsulta = urlDaConsulta,
                    TipoConsulta = tipoConsulta,
                    Entidade = entidade,
                    EntidadeId = entidadeId,
                    StatusCode = statusCode,
                    Resposta = resposta!
                });

                return Task.CompletedTask;
            }
        }

        private class ChamadaSalvarCache
        {
            public string ChaveCache { get; set; } = string.Empty;
            public string UrlDaConsulta { get; set; } = string.Empty;
            public string TipoConsulta { get; set; } = string.Empty;
            public string Entidade { get; set; } = string.Empty;
            public int? EntidadeId { get; set; }
            public int StatusCode { get; set; }
            public object Resposta { get; set; } = new();
        }
    }
}