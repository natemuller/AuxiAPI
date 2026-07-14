using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;

namespace AuxiAPI.src.Repositories
{
    public interface ICondominioRepository
    {
        Task<(List<AtlasCondominio> Itens, int TotalItens)> ListarAsync(
            VisualizarCondominioQuery query,
            int tamanhoPagina);

        Task<AtlasCondominio?> ObterPorCodCondomAsync(int codcondom);
    }
}