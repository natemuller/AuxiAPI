using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
using Moq;

namespace AuxiAPI.Tests.ServicesTest;

public class CondominioServiceCacheTest
{
    [Fact]
    public async Task ObterPorCodCondomAsync_DeveRetornarDoCache_NaSegundaConsulta()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .SetupSequence(x => x.ObterPorCodCondomAsync(5396))
            .ReturnsAsync(CreateAtlasCondominio())
            .ThrowsAsync(new Exception("Repository foi chamado de novo. Cache não funcionou."));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var primeiraConsulta = await service.ObterPorCodCondomAsync(5396);
        var segundaConsulta = await service.ObterPorCodCondomAsync(5396);

        repository.Verify(x => x.ObterPorCodCondomAsync(5396), Times.Once);

        Assert.Equal(5396, primeiraConsulta.CodCondom);
        Assert.Equal(5396, segundaConsulta.CodCondom);
        Assert.Equal(primeiraConsulta.NomeCondom, segundaConsulta.NomeCondom);
        Assert.Equal(primeiraConsulta.Cnpj, segundaConsulta.Cnpj);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveUsarMesmaChaveDeCache_QuandoNomeVariarAcento()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<AtlasCondominio>
            {
                CreateAtlasCondominio(nome: "Residencial Pelé")
            }, 1));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var primeiraQuery = new VisualizarCondominioQuery
        {
            NomeCondom = "pelé",
            Pagina = 1
        };

        var segundaQuery = new VisualizarCondominioQuery
        {
            NomeCondom = "pele",
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
            .ReturnsAsync((new List<AtlasCondominio>
            {
                CreateAtlasCondominio()
            }, 1))
            .ThrowsAsync(new Exception("Repository foi chamado de novo. Cache por nome não funcionou."));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var query = new VisualizarCondominioQuery
        {
            NomeCondom = "residencial"
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
            primeiraConsulta.Itens[0].NomeCondom,
            segundaConsulta.Itens[0].NomeCondom);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveUsarMesmaChaveDeCache_QuandoNomeVariarMaiusculaMinusculaEEspacos()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<AtlasCondominio>
            {
                CreateAtlasCondominio()
            }, 1));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var primeiraQuery = new VisualizarCondominioQuery
        {
            NomeCondom = "Residencial"
        };

        var segundaQuery = new VisualizarCondominioQuery
        {
            NomeCondom = " RESIDENCIAL "
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
            .ReturnsAsync((new List<AtlasCondominio>
            {
                CreateAtlasCondominio(codCondom: 5396, nome: "Residencial Página 1")
            }, 20))
            .ReturnsAsync((new List<AtlasCondominio>
            {
                CreateAtlasCondominio(codCondom: 5400, nome: "Residencial Página 2")
            }, 20));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var primeiraQuery = new VisualizarCondominioQuery
        {
            NomeCondom = "residencial",
            Pagina = 1
        };

        var segundaQuery = new VisualizarCondominioQuery
        {
            NomeCondom = "residencial",
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

        Assert.Equal(5396, primeiraConsulta.Itens[0].CodCondom);
        Assert.Equal(5400, segundaConsulta.Itens[0].CodCondom);

        Assert.Equal("Residencial Página 1", primeiraConsulta.Itens[0].NomeCondom);
        Assert.Equal("Residencial Página 2", segundaConsulta.Itens[0].NomeCondom);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveRetornarDoCache_NaSegundaConsultaPorCnpj()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .SetupSequence(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<AtlasCondominio>
            {
                CreateAtlasCondominio(cnpj: "17474690000113")
            }, 1))
            .ThrowsAsync(new Exception("Repository foi chamado de novo. Cache por CNPJ não funcionou."));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var primeiraQuery = new VisualizarCondominioQuery
        {
            Cnpj = "17.474.690/0001-13",
            Pagina = 1
        };

        var segundaQuery = new VisualizarCondominioQuery
        {
            Cnpj = "17474690000113",
            Pagina = 1
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

        Assert.Equal("17474690000113", primeiraConsulta.Itens[0].Cnpj);
        Assert.Equal("17474690000113", segundaConsulta.Itens[0].Cnpj);
    }

    [Fact]
    public async Task ListarCondominiosAsync_NaoDeveUsarCache_QuandoNomeVierComCnpj()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<AtlasCondominio>
            {
                CreateAtlasCondominio()
            }, 1));

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        var query = new VisualizarCondominioQuery
        {
            NomeCondom = "residencial",
            Cnpj = "12345678000101"
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
    public async Task ListarCondominiosAsync_NaoDeveUsarCache_QuandoNaoTiverFiltro()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<AtlasCondominio>
            {
                CreateAtlasCondominio()
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
    public async Task ObterPorCodCondomAsync_NaoDeveCachearErro_QuandoCodCondomNaoExistir()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ObterPorCodCondomAsync(9999))
            .ReturnsAsync((AtlasCondominio?)null);

        var cacheService = new FakeDatabaseCacheService();
        var service = CriarService(repository.Object, cacheService);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ObterPorCodCondomAsync(9999));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ObterPorCodCondomAsync(9999));

        repository.Verify(x => x.ObterPorCodCondomAsync(9999), Times.Exactly(2));

        Assert.Equal(2, cacheService.TotalDeBuscas);
        Assert.Equal(0, cacheService.TotalDeSalvamentos);
    }

    private static CondominioService CriarService(
        ICondominioRepository repository,
        IDatabaseCacheService cacheService)
    {
        return new CondominioService(repository, cacheService);
    }

    private static AtlasCondominio CreateAtlasCondominio(
        int codCondom = 5396,
        string nome = "Residencial Brasil-Hexa",
        string cnpj = "12345678000101")
    {
        return new AtlasCondominio
        {
            CodCondom = codCondom,
            NomeCondom = nome,
            Ativo = "S",
            Cnpj = cnpj,
            Cei = null,
            InscrMunicip = null,
            QtdBlocos = 1,
            QtdUnidades = 10,
            TotalFracao = 10000000000,
            DiaVencDoc = 10,
            DataInicioAdm = 43399,
            DataDistrato = null,
            MotivoDistrato = null,
            Assessor = "Gerente",
            Filial = "Porto Alegre",
            Agencia = "Agência Teste",
            Sindico = "Síndico",
            SubSindico = null,
            Conselheiro = null,
            Gestor = null,
            ConselhoFiscal = null,
            ConselhoConsultivo = null,
            ConselhoSuplente = null,
            TipoCondominio = "Residencial",
            TipoCategoria = "Condomínio",
            DtAlteracao = DateTime.UtcNow,
            TipoLograd = "Rua",
            Lograd = "Rua Teste",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "Porto Alegre",
            Cep8Log = "90000000",
            Uf = "RS",
            CodPessoaSindico = "123",
            NomeSindico = "Síndico Teste",
            CpfDocnpj = "00000000000",
            CondGarantido = "N",
            TipoConta = "Conta Corrente",
            ObsCobranca = null,
            Garantidora = null
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