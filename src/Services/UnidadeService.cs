using AuxiAPI.src.Repositories;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Common;
using AuxiAPI.src.Common.Text;
using AuxiAPI.src.Mappers;

namespace AuxiAPI.src.Services
{
    public class UnidadeService(
        IUnidadeRepository repository,
        IDatabaseCacheService cacheService)
    {
        private const int TamanhoPagina = 10;

        public async Task<ResultadoPaginadoDto<AtlasUnidadeDto>> ListarUnidadesAsync(
            VisualizarUnidadeQuery query)
        {
            ValidarFiltros(query);
            ValidarPaginacao(query);

            if (!DeveUsarCacheNaListagem(query))
                return await ListarSemCacheAsync(query);

            var dadosCache = MontarDadosCacheListagem(query);

            var cache = await cacheService
                .ObterAsync<ResultadoPaginadoDto<AtlasUnidadeDto>>(dadosCache.ChaveCache);

            if (cache is not null)
                return cache;

            var resultado = await ListarSemCacheAsync(query);

            await cacheService.SalvarAsync(
                dadosCache.ChaveCache,
                dadosCache.UrlDaConsulta,
                tipoConsulta: dadosCache.TipoConsulta,
                entidade: "atlas_unidades",
                entidadeId: null,
                resposta: resultado);

            return resultado;
        }

        public async Task<AtlasUnidadeDto> ObterPorIdEconomiaAsync(int idEconomia)
        {
            var urlDaConsulta = $"/api/unidades/{idEconomia}";
            var chaveCache = $"GET:{urlDaConsulta}";

            var cache = await cacheService.ObterAsync<AtlasUnidadeDto>(chaveCache);

            if (cache is not null)
                return cache;

            var unidade = await repository.ObterPorIdEconomiaAsync(idEconomia)
                ?? throw new KeyNotFoundException($"unidade com ideconomia {idEconomia} não foi encontrada.");

            var resultado = unidade.ToDto();

            await cacheService.SalvarAsync(
                chaveCache,
                urlDaConsulta,
                tipoConsulta: "UNIDADE_IDECONOMIA",
                entidade: "atlas_unidades",
                entidadeId: idEconomia,
                resposta: resultado);

            return resultado;
        }

        private async Task<ResultadoPaginadoDto<AtlasUnidadeDto>> ListarSemCacheAsync(
            VisualizarUnidadeQuery query)
        {
            var resultado = await repository.ListarAsync(query, TamanhoPagina);

            var itens = resultado.Itens
                .Select(u => u.ToDto())
                .ToList();

            return new ResultadoPaginadoDto<AtlasUnidadeDto>
            {
                Pagina = query.Pagina,
                TamanhoPagina = TamanhoPagina,
                TotalItens = resultado.TotalItens,
                TotalPaginas = (int)Math.Ceiling(resultado.TotalItens / (double)TamanhoPagina),
                Itens = itens
            };
        }

        private static bool DeveUsarCacheNaListagem(VisualizarUnidadeQuery query)
        {
            return DeveUsarCachePorCodCondom(query)
                || DeveUsarCachePorNomeCondomino(query);
        }

        private static bool DeveUsarCachePorCodCondom(VisualizarUnidadeQuery query)
        {
            return query.CodCondom.HasValue
                && string.IsNullOrWhiteSpace(query.NomeCondomino);
        }

        private static bool DeveUsarCachePorNomeCondomino(VisualizarUnidadeQuery query)
        {
            return !string.IsNullOrWhiteSpace(query.NomeCondomino)
                && !query.CodCondom.HasValue;
        }

        private static (string ChaveCache, string UrlDaConsulta, string TipoConsulta) MontarDadosCacheListagem(
            VisualizarUnidadeQuery query)
        {
            if (DeveUsarCachePorCodCondom(query))
            {
                var urlDaConsulta =
                    $"/api/unidades?codCondom={query.CodCondom}&pagina={query.Pagina}";

                return (
                    ChaveCache: $"GET:{urlDaConsulta}",
                    UrlDaConsulta: urlDaConsulta,
                    TipoConsulta: "UNIDADE_CODCONDOM"
                );
            }

            var nomeNormalizado = TextNormalizer.NormalizarBusca(query.NomeCondomino!.Trim());

            var urlPorNome =
                $"/api/unidades?nomeCondomino={Uri.EscapeDataString(nomeNormalizado)}&pagina={query.Pagina}";

            return (
                ChaveCache: $"GET:{urlPorNome}",
                UrlDaConsulta: urlPorNome,
                TipoConsulta: "UNIDADE_NOME_CONDOMINO"
            );
        }

        private static void ValidarFiltros(VisualizarUnidadeQuery query)
        {
            if (query.CodCondom.HasValue && query.CodCondom.Value <= 0)
                throw new ArgumentException("codigo do condomínio inválido.");

            if (query.NomeCondomino?.Length > 200)
                throw new ArgumentException(MensagensDeErro.NomeTamanhoExcedido);
        }

        private static void ValidarPaginacao(VisualizarUnidadeQuery query)
        {
            if (query.Pagina < 1)
                throw new ArgumentException(MensagensDeErro.PaginaInvalida);
        }
    }
}