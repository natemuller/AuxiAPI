using AuxiAPI.src.Common.Cache;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace AuxiAPI.Tests.ServicesTest;

public class CondominioServiceCacheTest
{
    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarDoCache_NaSegundaConsulta()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .SetupSequence(x => x.ObterPorIdAsync(1))
            .ReturnsAsync(CreateCondominio())
            .ThrowsAsync(new Exception("Repository foi chamado de novo. Cache não funcionou."));

        var service = CriarService(repository.Object);

        var primeiraConsulta = await service.ObterPorIdAsync(1);
        var segundaConsulta = await service.ObterPorIdAsync(1);

        repository.Verify(x => x.ObterPorIdAsync(1), Times.Once);

        Assert.Equal("0001", primeiraConsulta.CodigoDoCondominio);
        Assert.Equal("0001", segundaConsulta.CodigoDoCondominio);
        Assert.Equal(primeiraConsulta.NomeDoCondominio, segundaConsulta.NomeDoCondominio);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveRetornarDoCache_NaSegundaConsultaPorNome()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .SetupSequence(x => x.ListarAsync(It.IsAny<VisualizarCondominioQuery>()))
            .ReturnsAsync(new List<Condominio>
            {
                CreateCondominio()
            })
            .ThrowsAsync(new Exception("Repository foi chamado de novo. Cache por nome não funcionou."));

        var service = CriarService(repository.Object);

        var query = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "residencial"
        };

        var primeiraConsulta = await service.ListarCondominiosAsync(query);
        var segundaConsulta = await service.ListarCondominiosAsync(query);

        repository.Verify(
            x => x.ListarAsync(It.IsAny<VisualizarCondominioQuery>()),
            Times.Once);

        Assert.Single(primeiraConsulta);
        Assert.Single(segundaConsulta);
        Assert.Equal(
            primeiraConsulta[0].NomeDoCondominio,
            segundaConsulta[0].NomeDoCondominio);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveUsarMesmaChaveDeCache_QuandoNomeVariarMaiusculaMinusculaEEspacos()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(It.IsAny<VisualizarCondominioQuery>()))
            .ReturnsAsync(new List<Condominio>
            {
                CreateCondominio()
            });

        var service = CriarService(repository.Object);

        var primeiraQuery = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "Residencial"
        };

        var segundaQuery = new VisualizarCondominioQuery
        {
            NomeDoCondominio = " RESIDENCIAL "
        };

        var primeiraConsulta = await service.ListarCondominiosAsync(primeiraQuery);
        var segundaConsulta = await service.ListarCondominiosAsync(segundaQuery);

        repository.Verify(
            x => x.ListarAsync(It.IsAny<VisualizarCondominioQuery>()),
            Times.Once);

        Assert.Single(primeiraConsulta);
        Assert.Single(segundaConsulta);
    }

    [Fact]
    public async Task ListarCondominiosAsync_NaoDeveUsarCache_QuandoNomeVierComCodigo()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(It.IsAny<VisualizarCondominioQuery>()))
            .ReturnsAsync(new List<Condominio>
            {
                CreateCondominio()
            });

        var service = CriarService(repository.Object);

        var query = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "residencial",
            CodigoDoCondominio = "0001"
        };

        await service.ListarCondominiosAsync(query);
        await service.ListarCondominiosAsync(query);

        repository.Verify(
            x => x.ListarAsync(It.IsAny<VisualizarCondominioQuery>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ListarCondominiosAsync_NaoDeveUsarCache_QuandoNaoTiverFiltroDeNome()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(It.IsAny<VisualizarCondominioQuery>()))
            .ReturnsAsync(new List<Condominio>
            {
                CreateCondominio()
            });

        var service = CriarService(repository.Object);

        var query = new VisualizarCondominioQuery();

        await service.ListarCondominiosAsync(query);
        await service.ListarCondominiosAsync(query);

        repository.Verify(
            x => x.ListarAsync(It.IsAny<VisualizarCondominioQuery>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ObterPorIdAsync_NaoDeveCachearErro_QuandoIdNaoExistir()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ObterPorIdAsync(9999))
            .ReturnsAsync((Condominio?)null);

        var service = CriarService(repository.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ObterPorIdAsync(9999));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ObterPorIdAsync(9999));

        repository.Verify(x => x.ObterPorIdAsync(9999), Times.Exactly(2));
    }

    private static CondominioService CriarService(ICondominioRepository repository)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheService = new MemoryCacheService(memoryCache);

        return new CondominioService(repository, cacheService);
    }

    private static Condominio CreateCondominio()
    {
        return new Condominio
        {
            CodigoDoCondominio = "0001",
            CNPJDoCondominio = "12345678000101",
            NomeDoCondominio = "Residencial Brasil-Hexa",
            Endereco = "Rua Teste",
            NumeroDoEndereco = "123",
            EstadoDoEndereco = "RS",
            CidadeDoEndereco = "Porto Alegre",
            BairroDoEndereco = "Centro",
            CEPDoEndereco = "90000000",
            NumeroDeTorres = 1,
            NumeroDeUnidades = 10,
            Status = "Ativo",
            DataInicial_Administracao = "01/01/2024",
            DataFinal_Administracao = string.Empty,
            NomeGerenteDeContas = "Gerente",
            NomeSindico = "Síndico"
        };
    }
}