namespace AuxiAPI.src.Services
{
    public interface IDatabaseCacheService
    {
        Task<T?> ObterAsync<T>(string chaveCache) where T : class;

        Task SalvarAsync<T>(
            string chaveCache,
            string urlDaConsulta,
            string tipoConsulta,
            string entidade,
            int? entidadeId,
            T resposta,
            int statusCode = 200) where T : class;
    }
}