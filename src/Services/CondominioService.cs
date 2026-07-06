using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Common;
using AuxiAPI.src.Common.Cache;

namespace AuxiAPI.src.Services
{
    public class CondominioService(ICondominioRepository repository, ICacheService cacheService)
    {
        private static readonly TimeSpan TempoCachePorId = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan TempoCachePorNome = TimeSpan.FromMinutes(2);
        
        public async Task<List<InformacoesCondominioDto>> ListarCondominiosAsync(VisualizarCondominioQuery query)
        {
            ValidarFiltros(query);

            if (!DeveUsarCachePorNome(query))
                return await ListarSemCacheAsync(query);

            var nomeNormalizado = query.NomeDoCondominio!.Trim().ToLowerInvariant();
            var cacheKey = CondominioCacheKeys.PorNome(nomeNormalizado);

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

        private async Task<List<InformacoesCondominioDto>> ListarSemCacheAsync(VisualizarCondominioQuery query)
        {
            var condominios = await repository.ListarAsync(query);
            return condominios
                .Select(MapearParaDto)
                .ToList();
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