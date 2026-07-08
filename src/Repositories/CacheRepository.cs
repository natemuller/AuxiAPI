using AuxiAPI.src.Contexts;
using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.src.Repositories
{
    public class CacheRepository(CondominiosDbContext context) : ICacheRepository
    {
        public async Task<CacheEntry?> ObterValidoPorChaveAsync(string chaveCache)
        {
            var agora = DateTime.UtcNow;

            return await context.Cache
                .AsNoTracking()
                .Where(c =>
                    c.ChaveCache == chaveCache &&
                    c.ExpiradoEm > agora &&
                    c.InvalidadoEm == null)
                .OrderByDescending(c => c.CriadoEm)
                .FirstOrDefaultAsync();
        }

        public async Task SalvarAsync(CacheEntry cacheEntry)
        {
            context.Cache.Add(cacheEntry);
            await context.SaveChangesAsync();
        }
    }
}