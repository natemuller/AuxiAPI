using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Common;
using AuxiAPI.src.Common.Cache;
using AuxiAPI.src.Common.Text;

namespace AuxiAPI.src.Services
{
    public class CondominioService(ICondominioRepository repository, ICacheService cacheService)
    {
        private const int TamanhoPagina = 10;
        private static readonly TimeSpan TempoCachePorId = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan TempoCachePorNome = TimeSpan.FromMinutes(2);
        
        public async Task<ResultadoPaginadoDto<InformacoesCondominioDto>> ListarCondominiosAsync(
            VisualizarCondominioQuery query)
        {
            ValidarFiltros(query);
            ValidarPaginacao(query);

            if (!DeveUsarCachePorNome(query))
                return await ListarSemCacheAsync(query);

            var nomeNormalizado = TextNormalizer.NormalizarBusca(query.NomeDoCondominio);
            var cacheKey = CondominioCacheKeys.PorNome(nomeNormalizado, query.Pagina);

            return await cacheService.GetOrCreateAsync(
                cacheKey,
                TempoCachePorNome,
                () => ListarSemCacheAsync(query));
        }

        public async Task<InformacoesCondominioDto> ObterPorIdAsync(int id)
        {
            var cacheKey = CondominioCacheKeys.PorId(id);

            return await cacheService.GetOrCreateAsync(
                cacheKey,
                TempoCachePorId,
                async () =>
                {
                    var condominio = await repository.ObterPorIdAsync(id)
                        ?? throw new KeyNotFoundException($"condomínio com id {id} não foi encontrado.");

                    return MapearParaDto(condominio);
                });
        }

        private async Task<ResultadoPaginadoDto<InformacoesCondominioDto>> ListarSemCacheAsync(
            VisualizarCondominioQuery query)
        {
            var resultado = await repository.ListarAsync(query, TamanhoPagina);

            var itens = resultado.Itens
                .Select(MapearParaDto)
                .ToList();

            return new ResultadoPaginadoDto<InformacoesCondominioDto>
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
            return !string.IsNullOrWhiteSpace(query.NomeDoCondominio)
                && string.IsNullOrWhiteSpace(query.CodigoDoCondominio)
                && string.IsNullOrWhiteSpace(query.CNPJDoCondominio);
        }

        private static void ValidarFiltros(VisualizarCondominioQuery query)
        {
            if (query.CodigoDoCondominio?.Length > 15)
                throw new ArgumentException(MensagensDeErro.CodigoTamanhoExcedido);

            if (query.CNPJDoCondominio?.Length > 15)
                throw new ArgumentException(MensagensDeErro.CnpjTamanhoExcedido);

            if (query.NomeDoCondominio?.Length > 200)
                throw new ArgumentException(MensagensDeErro.NomeTamanhoExcedido);
        }

        private static void ValidarPaginacao(VisualizarCondominioQuery query)
        {
            if (query.Pagina <1) throw new ArgumentException(MensagensDeErro.PaginaInvalida);
        }

        private static InformacoesCondominioDto MapearParaDto(Condominio condominio)
        {
            return new InformacoesCondominioDto
            {
                CodigoDoCondominio = condominio.CodigoDoCondominio,
                CNPJDoCondominio = condominio.CNPJDoCondominio,
                NomeDoCondominio = condominio.NomeDoCondominio,
                Endereco = condominio.Endereco,
                NumeroDoEndereco = condominio.NumeroDoEndereco,
                EstadoDoEndereco = condominio.EstadoDoEndereco,
                CidadeDoEndereco = condominio.CidadeDoEndereco,
                BairroDoEndereco = condominio.BairroDoEndereco,
                CEPDoEndereco = condominio.CEPDoEndereco,
                NumeroDeTorres = condominio.NumeroDeTorres,
                NumeroDeUnidades = condominio.NumeroDeUnidades,
                Status = condominio.Status,
                DataInicial_Administracao = condominio.DataInicial_Administracao,
                DataFinal_Administracao = condominio.DataFinal_Administracao,
                NomeGerenteDeContas = condominio.NomeGerenteDeContas,
                NomeSindico = condominio.NomeSindico
            };
        }
    }
}