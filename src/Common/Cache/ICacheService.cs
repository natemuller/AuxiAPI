namespace AuxiAPI.src.Common.Cache
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(
            string key,
            TimeSpan expiration,
            Func<Task<T>> factory);
    }
}