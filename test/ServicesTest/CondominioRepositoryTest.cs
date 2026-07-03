using System.IO;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;

namespace ServicesTest;

public class CondominioRepositoryTest
{
    [Fact]
    public void LerJson_QuandoArquivoJsonLocalExiste_DeveCarregarOsCondominios()
    {
        var caminhoDoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "CondominiosTeste.json");
        File.WriteAllText(caminhoDoArquivo, "[{\"codigoDoCondominio\":\"0001\",\"nomeDoCondominio\":\"Residencial Teste\"}]");

        try
        {
            var repository = new CondominioRepository(caminhoDoArquivo);

            var condominios = repository.LerJson();

            var condominio = Assert.Single(condominios);
            Assert.Equal("0001", condominio.CodigoDoCondominio);
            Assert.Equal("Residencial Teste", condominio.NomeDoCondominio);
        }
        finally
        {
            if (File.Exists(caminhoDoArquivo))
            {
                File.Delete(caminhoDoArquivo);
            }
        }
    }
}
