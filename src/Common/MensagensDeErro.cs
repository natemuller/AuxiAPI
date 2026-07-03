namespace AuxiAPI.src.Common
{
    public static class MensagensDeErro
    {
        public const string CodigoTamanhoExcedido = "o codigo de busca nao pode passar de 15 caracteres.";        
        public const string CnpjTamanhoExcedido = "cnpj de busca nao pode passar de 15 caracteres.";
        public const string NomeTamanhoExcedido = "o nome de busca nao pode passar de 200 caracteres.";
            
        public const string Erro404 = "o recurso solicitado nao foi encontrado.";
        public const string Erro500 = "desculpe, ocorreu um erro interno no servidor, tente novamente mais tarde.";
    }
}