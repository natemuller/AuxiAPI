using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
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

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var primeiraConsulta = await service.ObterPorIdAsync(1);
        var segundaConsulta = await service.ObterPorIdAsync(1);

        repository.Verify(x => x.ObterPorIdAsync(1), Times.Once);

        Assert.Equal("0001", primeiraConsulta.CodigoDoCondominio);
        Assert.Equal("0001", segundaConsulta.CodigoDoCondominio);
        Assert.Equal(primeiraConsulta.NomeDoCondominio, segundaConsulta.NomeDoCondominio);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveUsarMesmaChaveDeCache_QuandoNomeVariarAcento()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio>
            {
                CreateCondominio("0001", "Residencial Pelé")
            }, 1));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var primeiraQuery = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "pelé",
            Pagina = 1
        };

        var segundaQuery = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "pele",
            Pagina = 1
        };

        await service.ListarCondominiosAsync(primeiraQuery);
        await service.ListarCondominiosAsync(segundaQuery);

        repository.Verify(
            x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveRetornarDoCache_NaSegundaConsultaPorNome()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .SetupSequence(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio>
            {
                CreateCondominio()
            }, 1))
            .ThrowsAsync(new Exception("Repository foi chamado de novo. Cache por nome não funcionou."));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var query = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "residencial"
        };

        var primeiraConsulta = await service.ListarCondominiosAsync(query);
        var segundaConsulta = await service.ListarCondominiosAsync(query);

        repository.Verify(
            x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()),
            Times.Once);

        Assert.Single(primeiraConsulta.Itens);
        Assert.Single(segundaConsulta.Itens);

        Assert.Equal(1, primeiraConsulta.Pagina);
        Assert.Equal(10, primeiraConsulta.TamanhoPagina);
        Assert.Equal(1, primeiraConsulta.TotalItens);
        Assert.Equal(1, primeiraConsulta.TotalPaginas);

        Assert.Equal(
            primeiraConsulta.Itens[0].NomeDoCondominio,
            segundaConsulta.Itens[0].NomeDoCondominio);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveUsarMesmaChaveDeCache_QuandoNomeVariarMaiusculaMinusculaEEspacos()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio>
            {
                CreateCondominio()
            }, 1));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

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
            x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()),
            Times.Once);

        Assert.Single(primeiraConsulta.Itens);
        Assert.Single(segundaConsulta.Itens);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveUsarCachesDiferentes_QuandoPaginasForemDiferentes()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .SetupSequence(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio>
            {
                CreateCondominio("0001", "Residencial Página 1")
            }, 20))
            .ReturnsAsync((new List<Condominio>
            {
                CreateCondominio("0011", "Residencial Página 2")
            }, 20));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var primeiraQuery = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "residencial",
            Pagina = 1
        };

        var segundaQuery = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "residencial",
            Pagina = 2
        };

        var primeiraConsulta = await service.ListarCondominiosAsync(primeiraQuery);
        var segundaConsulta = await service.ListarCondominiosAsync(segundaQuery);

        repository.Verify(
            x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()),
            Times.Exactly(2));

        Assert.Equal(1, primeiraConsulta.Pagina);
        Assert.Equal(2, segundaConsulta.Pagina);

        Assert.Equal("0001", primeiraConsulta.Itens[0].CodigoDoCondominio);
        Assert.Equal("0011", segundaConsulta.Itens[0].CodigoDoCondominio);

        Assert.Equal("Residencial Página 1", primeiraConsulta.Itens[0].NomeDoCondominio);
        Assert.Equal("Residencial Página 2", segundaConsulta.Itens[0].NomeDoCondominio);
    }

    [Fact]
    public async Task ListarCondominiosAsync_NaoDeveUsarCache_QuandoNomeVierComCodigo()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio>
            {
                CreateCondominio()
            }, 1));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var query = new VisualizarCondominioQuery
        {
            NomeDoCondominio = "residencial",
            CodigoDoCondominio = "0001"
        };

        await service.ListarCondominiosAsync(query);
        await service.ListarCondominiosAsync(query);

        repository.Verify(
            x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()),
            Times.Exactly(2));

        Assert.Equal(0, cacheService.TotalDeBuscas);
        Assert.Equal(0, cacheService.TotalDeSalvamentos);
    }

    [Fact]
    public async Task ListarCondominiosAsync_NaoDeveUsarCache_QuandoNaoTiverFiltroDeNome()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio>
            {
                CreateCondominio()
            }, 1));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var query = new VisualizarCondominioQuery();

        await service.ListarCondominiosAsync(query);
        await service.ListarCondominiosAsync(query);

        repository.Verify(
            x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()),
            Times.Exactly(2));

        Assert.Equal(0, cacheService.TotalDeBuscas);
        Assert.Equal(0, cacheService.TotalDeSalvamentos);
    }

    [Fact]
    public async Task ObterPorIdAsync_NaoDeveCachearErro_QuandoIdNaoExistir()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ObterPorIdAsync(9999))
            .ReturnsAsync((Condominio?)null);

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ObterPorIdAsync(9999));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ObterPorIdAsync(9999));

        repository.Verify(x => x.ObterPorIdAsync(9999), Times.Exactly(2));

        Assert.Equal(2, cacheService.TotalDeBuscas);
        Assert.Equal(0, cacheService.TotalDeSalvamentos);
    }

    private static CondominioService CriarService(
        ICondominioRepository repository,
        IDatabaseCacheService cacheService)
    {
        return new CondominioService(repository, cacheService);
    }

    private static Condominio CreateCondominio(
        string codigo = "0001",
        string nome = "Residencial Brasil-Hexa")
    {
        return new Condominio
        {
            CodigoDoCondominio = codigo,
            CNPJDoCondominio = "12345678000101",
            NomeDoCondominio = nome,
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

    private sealed class FakeDatabaseCacheService : IDatabaseCacheService
    {
        private readonly Dictionary<string, object> _cache = new();

        public int TotalDeBuscas { get; private set; }
        public int TotalDeSalvamentos { get; private set; }

        public Task<T?> ObterAsync<T>(string chaveCache) where T : class
        {
            TotalDeBuscas++;

            if (_cache.TryGetValue(chaveCache, out var resposta)
                && resposta is T respostaTipada)
            {
                return Task.FromResult<T?>(respostaTipada);
            }

            return Task.FromResult<T?>(null);
        }

        public Task SalvarAsync<T>(
            string chaveCache,
            string urlDaConsulta,
            string tipoConsulta,
            string entidade,
            int? entidadeId,
            T resposta,
            int statusCode = 200) where T : class
        {
            TotalDeSalvamentos++;
            _cache[chaveCache] = resposta;

            return Task.CompletedTask;
        }
    }
}