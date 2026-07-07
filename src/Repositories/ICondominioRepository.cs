using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;

namespace AuxiAPI.src.Repositories
{
    public interface ICondominioRepository
    {
        Task<(List<Condominio> Itens, int TotalItens)> ListarAsync(VisualizarCondominioQuery query, int tamanhoPagina);
        Task<Condominio?> ObterPorIdAsync(int id);
    }
}