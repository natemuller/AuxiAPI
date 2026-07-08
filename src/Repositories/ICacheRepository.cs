using AuxiAPI.src.Entities;

namespace AuxiAPI.src.Repositories
{
    public interface ICacheRepository
    {
        Task<CacheEntry?> ObterValidoPorChaveAsync(string chaveCache);
        Task SalvarAsync(CacheEntry cacheEntry);
    }
}