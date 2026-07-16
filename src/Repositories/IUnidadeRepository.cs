using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;

namespace AuxiAPI.src.Repositories
{
    public interface IUnidadeRepository
    {
        Task<(List<AtlasUnidade> Itens, int TotalItens)> ListarAsync(
            VisualizarUnidadeQuery query,
            int tamanhoPagina);

        Task<AtlasUnidade?> ObterPorIdEconomiaAsync(int idEconomia);
    }
}