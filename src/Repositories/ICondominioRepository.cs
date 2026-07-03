using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;

namespace AuxiAPI.src.Repositories
{
    public interface ICondominioRepository
    {
        Task<List<Condominio>> ListarAsync(VisualizarCondominioQuery query);
        Task<Condominio?> ObterPorIdAsync(int id);
    }
}