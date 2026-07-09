namespace AuxiAPI.src.Entities
{
    public class CacheEntry
    {
        public Guid Id { get; set;}
        public string ChaveCache {get; set;} = string.Empty;
        public string UrlDaConsulta {get; set;} = string.Empty;
        public string MetodoHttp {get; set;} = "GET";
        public string TipoConsulta {get; set;} = string.Empty;
        public string Entidade {get; set;} = string.Empty;
        public int? EntidadeId {get; set;}
        public string Resposta {get; set;} = string.Empty;
        public int StatusCode {get; set;}
        public DateTime CriadoEm {get; set;}
        public DateTime ExpiradoEm {get; set;}
        public DateTime? InvalidadoEm {get; set;}
        public string? MotivoInvalidacao {get; set;}
   }
}