using System.Text.Json;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
using Moq;

namespace AuxiAPI.Tests.ServicesTest;

public class DatabaseCacheServiceTest
{
    [Fact]
    public async Task ObterAsync_DeveRetornarNull_QuandoCacheNaoExistir()
    {
        var repository = new Mock<ICacheRepository>();

        repository
            .Setup(x => x.ObterValidoPorChaveAsync("GET:/api/condominios/5396"))
            .ReturnsAsync((CacheEntry?)null);

        var service = new DatabaseCacheService(repository.Object);

        var resultado = await service.ObterAsync<AtlasCondominioDto>("GET:/api/condominios/5396");

        Assert.Null(resultado);

        repository.Verify(
            x => x.ObterValidoPorChaveAsync("GET:/api/condominios/5396"),
            Times.Once);
    }

    [Fact]
    public async Task ObterAsync_DeveDesserializarResposta_QuandoCacheExistir()
    {
        var repository = new Mock<ICacheRepository>();

        var dto = CreateDto();

        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var cacheEntry = new CacheEntry
        {
            Id = Guid.NewGuid(),
            ChaveCache = "GET:/api/condominios/5396",
            UrlDaConsulta = "/api/condominios/5396",
            MetodoHttp = "GET",
            TipoConsulta = "CONDOMINIO_CODCONDOM",
            Entidade = "atlas_condominios",
            EntidadeId = 5396,
            Resposta = json,
            StatusCode = 200,
            CriadoEm = DateTime.UtcNow,
            ExpiradoEm = DateTime.UtcNow.AddMinutes(15),
            InvalidadoEm = null,
            MotivoInvalidacao = null
        };

        repository
            .Setup(x => x.ObterValidoPorChaveAsync("GET:/api/condominios/5396"))
            .ReturnsAsync(cacheEntry);

        var service = new DatabaseCacheService(repository.Object);

        var resultado = await service.ObterAsync<AtlasCondominioDto>("GET:/api/condominios/5396");

        Assert.NotNull(resultado);
        Assert.Equal(5396, resultado!.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", resultado.NomeCondom);
        Assert.Equal("17474690000113", resultado.Cnpj);

        repository.Verify(
            x => x.ObterValidoPorChaveAsync("GET:/api/condominios/5396"),
            Times.Once);
    }

    [Fact]
    public async Task SalvarAsync_DeveCriarCacheEntryComDadosCorretos()
    {
        var repository = new Mock<ICacheRepository>();

        CacheEntry? cacheEntryCapturado = null;

        repository
            .Setup(x => x.SalvarAsync(It.IsAny<CacheEntry>()))
            .Callback<CacheEntry>(entry => cacheEntryCapturado = entry)
            .Returns(Task.CompletedTask);

        var service = new DatabaseCacheService(repository.Object);

        var dto = CreateDto();

        var antes = DateTime.UtcNow;

        await service.SalvarAsync(
            chaveCache: "GET:/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidade: "atlas_condominios",
            entidadeId: 5396,
            resposta: dto);

        var depois = DateTime.UtcNow;

        Assert.NotNull(cacheEntryCapturado);

        Assert.NotEqual(Guid.Empty, cacheEntryCapturado!.Id);
        Assert.Equal("GET:/api/condominios/5396", cacheEntryCapturado.ChaveCache);
        Assert.Equal("/api/condominios/5396", cacheEntryCapturado.UrlDaConsulta);
        Assert.Equal("GET", cacheEntryCapturado.MetodoHttp);
        Assert.Equal("CONDOMINIO_CODCONDOM", cacheEntryCapturado.TipoConsulta);
        Assert.Equal("atlas_condominios", cacheEntryCapturado.Entidade);
        Assert.Equal(5396, cacheEntryCapturado.EntidadeId);
        Assert.Equal(200, cacheEntryCapturado.StatusCode);
        Assert.Null(cacheEntryCapturado.InvalidadoEm);
        Assert.Null(cacheEntryCapturado.MotivoInvalidacao);

        Assert.InRange(cacheEntryCapturado.CriadoEm, antes, depois);

        var diferencaExpiracao = cacheEntryCapturado.ExpiradoEm - cacheEntryCapturado.CriadoEm;
        Assert.InRange(diferencaExpiracao.TotalMinutes, 14.99, 15.01);

        var respostaDesserializada = JsonSerializer.Deserialize<AtlasCondominioDto>(
            cacheEntryCapturado.Resposta,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(respostaDesserializada);
        Assert.Equal(dto.CodCondom, respostaDesserializada!.CodCondom);
        Assert.Equal(dto.NomeCondom, respostaDesserializada.NomeCondom);
        Assert.Equal(dto.Cnpj, respostaDesserializada.Cnpj);

        repository.Verify(
            x => x.SalvarAsync(It.IsAny<CacheEntry>()),
            Times.Once);
    }

    [Fact]
    public async Task SalvarAsync_DevePermitirStatusCodeCustomizado()
    {
        var repository = new Mock<ICacheRepository>();

        CacheEntry? cacheEntryCapturado = null;

        repository
            .Setup(x => x.SalvarAsync(It.IsAny<CacheEntry>()))
            .Callback<CacheEntry>(entry => cacheEntryCapturado = entry)
            .Returns(Task.CompletedTask);

        var service = new DatabaseCacheService(repository.Object);

        await service.SalvarAsync(
            chaveCache: "GET:/api/condominios?nomeCondom=solar&pagina=1",
            urlDaConsulta: "/api/condominios?nomeCondom=solar&pagina=1",
            tipoConsulta: "CONDOMINIO_NOME",
            entidade: "atlas_condominios",
            entidadeId: null,
            resposta: new ResultadoPaginadoDto<AtlasCondominioDto>(),
            statusCode: 200);

        Assert.NotNull(cacheEntryCapturado);
        Assert.Equal(200, cacheEntryCapturado!.StatusCode);
        Assert.Equal("CONDOMINIO_NOME", cacheEntryCapturado.TipoConsulta);
        Assert.Equal("atlas_condominios", cacheEntryCapturado.Entidade);
        Assert.Null(cacheEntryCapturado.EntidadeId);

        repository.Verify(
            x => x.SalvarAsync(It.IsAny<CacheEntry>()),
            Times.Once);
    }

    [Fact]
    public async Task SalvarAsync_DevePermitirCachePorCnpjComEntidadeIdNull()
    {
        var repository = new Mock<ICacheRepository>();

        CacheEntry? cacheEntryCapturado = null;

        repository
            .Setup(x => x.SalvarAsync(It.IsAny<CacheEntry>()))
            .Callback<CacheEntry>(entry => cacheEntryCapturado = entry)
            .Returns(Task.CompletedTask);

        var service = new DatabaseCacheService(repository.Object);

        await service.SalvarAsync(
            chaveCache: "GET:/api/condominios?cnpj=17474690000113&pagina=1",
            urlDaConsulta: "/api/condominios?cnpj=17474690000113&pagina=1",
            tipoConsulta: "CONDOMINIO_CNPJ",
            entidade: "atlas_condominios",
            entidadeId: null,
            resposta: new ResultadoPaginadoDto<AtlasCondominioDto>(),
            statusCode: 200);

        Assert.NotNull(cacheEntryCapturado);
        Assert.Equal("CONDOMINIO_CNPJ", cacheEntryCapturado!.TipoConsulta);
        Assert.Equal("atlas_condominios", cacheEntryCapturado.Entidade);
        Assert.Null(cacheEntryCapturado.EntidadeId);
        Assert.Equal("/api/condominios?cnpj=17474690000113&pagina=1", cacheEntryCapturado.UrlDaConsulta);

        repository.Verify(
            x => x.SalvarAsync(It.IsAny<CacheEntry>()),
            Times.Once);
    }

    private static AtlasCondominioDto CreateDto()
    {
        return new AtlasCondominioDto
        {
            CodCondom = 5396,
            NomeCondom = "SOLAR DI TOSCANA",
            Ativo = "S",
            Cnpj = "17474690000113",
            Cei = null,
            InscrMunicip = null,
            QtdBlocos = 1,
            QtdUnidades = 9,
            TotalFracao = 10000000000,
            DiaVencDoc = 10,
            DataInicioAdm = 43399,
            DataDistrato = null,
            MotivoDistrato = null,
            Assessor = "GERENTE TESTE",
            Filial = "PORTO ALEGRE",
            Agencia = "AGENCIA TESTE",
            Sindico = "SINDICO TESTE",
            SubSindico = null,
            Conselheiro = null,
            Gestor = null,
            ConselhoFiscal = null,
            ConselhoConsultivo = null,
            ConselhoSuplente = null,
            TipoCondominio = "Residencial",
            TipoCategoria = "Condomínio",
            DtAlteracao = DateTime.UtcNow,
            TipoLograd = "RUA",
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
}