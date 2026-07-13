using AuxiAPI.src.Repositories;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Common;
using AuxiAPI.src.Common.Text;
using AuxiAPI.src.Mappers;

namespace AuxiAPI.src.Services
{
    public class CondominioService(
        ICondominioRepository repository,
        IDatabaseCacheService cacheService)
    {
        private const int TamanhoPagina = 10;

        public async Task<ResultadoPaginadoDto<AtlasCondominioDto>> ListarCondominiosAsync(
            VisualizarCondominioQuery query)
        {
            ValidarFiltros(query);
            ValidarPaginacao(query);

            if (!DeveUsarCachePorNome(query))
                return await ListarSemCacheAsync(query);

            var nomeNormalizado = TextNormalizer.NormalizarBusca(query.NomeCondom!.Trim());

            var urlDaConsulta =
                $"/api/condominios?nomeCondom={Uri.EscapeDataString(nomeNormalizado)}&pagina={query.Pagina}";

            var chaveCache = $"GET:{urlDaConsulta}";

            var cache = await cacheService
                .ObterAsync<ResultadoPaginadoDto<AtlasCondominioDto>>(chaveCache);

            if (cache is not null)
                return cache;

            var resultado = await ListarSemCacheAsync(query);

            await cacheService.SalvarAsync(
                chaveCache,
                urlDaConsulta,
                tipoConsulta: "CONDOMINIO_NOME",
                entidade: "atlas_condominios",
                entidadeId: null,
                resposta: resultado);

            return resultado;
        }

        public async Task<AtlasCondominioDto> ObterPorIdAsync(int id)
        {
            var urlDaConsulta = $"/api/condominios/{id}";
            var chaveCache = $"GET:{urlDaConsulta}";

            var cache = await cacheService.ObterAsync<AtlasCondominioDto>(chaveCache);

            if (cache is not null)
                return cache;

            var condominio = await repository.ObterPorIdAsync(id)
                ?? throw new KeyNotFoundException($"condomínio com codcondom {id} não foi encontrado.");

            var resultado = condominio.ToDto();

            await cacheService.SalvarAsync(
                chaveCache,
                urlDaConsulta,
                tipoConsulta: "CONDOMINIO_ID",
                entidade: "atlas_condominios",
                entidadeId: id,
                resposta: resultado);

            return resultado;
        }

        private async Task<ResultadoPaginadoDto<AtlasCondominioDto>> ListarSemCacheAsync(
            VisualizarCondominioQuery query)
        {
            var resultado = await repository.ListarAsync(query, TamanhoPagina);

            var itens = resultado.Itens
                .Select(c => c.ToDto())
                .ToList();

            return new ResultadoPaginadoDto<AtlasCondominioDto>
            {
                Pagina = query.Pagina,
                TamanhoPagina = TamanhoPagina,
                TotalItens = resultado.TotalItens,
                TotalPaginas = (int)Math.Ceiling(resultado.TotalItens / (double)TamanhoPagina),
                Itens = itens
            };
        }

        private static bool DeveUsarCachePorNome(VisualizarCondominioQuery query)
        {
            return !string.IsNullOrWhiteSpace(query.NomeCondom)
                && string.IsNullOrWhiteSpace(query.Cnpj);
        }

        private static void ValidarFiltros(VisualizarCondominioQuery query)
        {
            if (query.Cnpj?.Length > 15)
                throw new ArgumentException(MensagensDeErro.CnpjTamanhoExcedido);

            if (query.NomeCondom?.Length > 200)
                throw new ArgumentException(MensagensDeErro.NomeTamanhoExcedido);
        }

        private static void ValidarPaginacao(VisualizarCondominioQuery query)
        {
            if (query.Pagina < 1)
                throw new ArgumentException(MensagensDeErro.PaginaInvalida);
        }
    }
}