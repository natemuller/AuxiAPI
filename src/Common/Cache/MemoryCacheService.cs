using Microsoft.Extensions.Caching.Memory;

namespace AuxiAPI.src.Common.Cache
{
    public class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
    {
        public async Task<T> GetOrCreateAsync<T>(
            string key,
            TimeSpan expiration,
            Func<Task<T>> factory)
        {
            if (memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
                return cachedValue;

            var value = await factory();

            memoryCache.Set(
                key,
                value,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                });

            return value;
        }
    }
}