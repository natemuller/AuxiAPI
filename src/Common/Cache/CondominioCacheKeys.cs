namespace AuxiAPI.src.Common.Cache
{
    public static class CondominioCacheKeys
    {
        public static string PorId(int id) =>
            $"condominios:id:{id}";

        public static string PorNome(string nomeNormalizado, int pagina) =>
            $"condominios:nome:{nomeNormalizado}:pagina:{pagina}";
    }
}