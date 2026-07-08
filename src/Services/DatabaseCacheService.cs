using System.Text.Json;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;

namespace AuxiAPI.src.Services
{
    public class DatabaseCacheService(ICacheRepository cacheRepository) : IDatabaseCacheService
    {
        private static readonly TimeSpan TempoExpiracao = TimeSpan.FromMinutes(15);

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task<T?> ObterAsync<T>(string chaveCache) where T : class
        {
            var cache = await cacheRepository.ObterValidoPorChaveAsync(chaveCache);

            if (cache is null)
                return null;

            return JsonSerializer.Deserialize<T>(cache.Resposta, JsonOptions);
        }

        public async Task SalvarAsync<T>(
            string chaveCache,
            string urlDaConsulta,
            string tipoConsulta,
            string entidade,
            int? entidadeId,
            T resposta,
            int statusCode = 200) where T : class
        {
            var agora = DateTime.UtcNow;

            var cacheEntry = new CacheEntry
            {
                Id = Guid.NewGuid(),
                ChaveCache = chaveCache,
                UrlDaConsulta = urlDaConsulta,
                MetodoHttp = "GET",
                TipoConsulta = tipoConsulta,
                Entidade = entidade,
                EntidadeId = entidadeId,
                Resposta = JsonSerializer.Serialize(resposta, JsonOptions),
                StatusCode = statusCode,
                CriadoEm = agora,
                ExpiradoEm = agora.Add(TempoExpiracao),
                InvalidadoEm = null,
                MotivoInvalidacao = null
            };

            await cacheRepository.SalvarAsync(cacheEntry);
        }
    }
}