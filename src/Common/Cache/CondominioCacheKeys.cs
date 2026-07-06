namespace AuxiAPI.src.Common.Cache
{
    public static class CondominioCacheKeys
    {
        public static string PorId(int id)
        {
            return $"condominios:id:{id}";
        }

        public static string PorNome(string nomeNormalizado)
        {
            return $"condominios:nome:{nomeNormalizado}";
        }
    }
}