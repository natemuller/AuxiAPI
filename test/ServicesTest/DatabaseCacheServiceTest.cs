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
            .Setup(x => x.ObterValidoPorChaveAsync("GET:/api/condominios/1"))
            .ReturnsAsync((CacheEntry?)null);

        var service = new DatabaseCacheService(repository.Object);

        var resultado = await service.ObterAsync<InformacoesCondominioDto>("GET:/api/condominios/1");

        Assert.Null(resultado);

        repository.Verify(
            x => x.ObterValidoPorChaveAsync("GET:/api/condominios/1"),
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
            ChaveCache = "GET:/api/condominios/1",
            UrlDaConsulta = "/api/condominios/1",
            MetodoHttp = "GET",
            TipoConsulta = "CONDOMINIO_ID",
            Entidade = "condominios",
            EntidadeId = 1,
            Resposta = json,
            StatusCode = 200,
            CriadoEm = DateTime.UtcNow,
            ExpiradoEm = DateTime.UtcNow.AddMinutes(15),
            InvalidadoEm = null,
            MotivoInvalidacao = null
        };

        repository
            .Setup(x => x.ObterValidoPorChaveAsync("GET:/api/condominios/1"))
            .ReturnsAsync(cacheEntry);

        var service = new DatabaseCacheService(repository.Object);

        var resultado = await service.ObterAsync<InformacoesCondominioDto>("GET:/api/condominios/1");

        Assert.NotNull(resultado);
        Assert.Equal("0001", resultado!.CodigoDoCondominio);
        Assert.Equal("Residencial Brasil-Hexa", resultado.NomeDoCondominio);

        repository.Verify(
            x => x.ObterValidoPorChaveAsync("GET:/api/condominios/1"),
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
            chaveCache: "GET:/api/condominios/1",
            urlDaConsulta: "/api/condominios/1",
            tipoConsulta: "CONDOMINIO_ID",
            entidade: "condominios",
            entidadeId: 1,
            resposta: dto);

        var depois = DateTime.UtcNow;

        Assert.NotNull(cacheEntryCapturado);

        Assert.NotEqual(Guid.Empty, cacheEntryCapturado!.Id);
        Assert.Equal("GET:/api/condominios/1", cacheEntryCapturado.ChaveCache);
        Assert.Equal("/api/condominios/1", cacheEntryCapturado.UrlDaConsulta);
        Assert.Equal("GET", cacheEntryCapturado.MetodoHttp);
        Assert.Equal("CONDOMINIO_ID", cacheEntryCapturado.TipoConsulta);
        Assert.Equal("condominios", cacheEntryCapturado.Entidade);
        Assert.Equal(1, cacheEntryCapturado.EntidadeId);
        Assert.Equal(200, cacheEntryCapturado.StatusCode);
        Assert.Null(cacheEntryCapturado.InvalidadoEm);
        Assert.Null(cacheEntryCapturado.MotivoInvalidacao);

        Assert.InRange(cacheEntryCapturado.CriadoEm, antes, depois);

        var diferencaExpiracao = cacheEntryCapturado.ExpiradoEm - cacheEntryCapturado.CriadoEm;
        Assert.InRange(diferencaExpiracao.TotalMinutes, 14.99, 15.01);

        var respostaDesserializada = JsonSerializer.Deserialize<InformacoesCondominioDto>(
            cacheEntryCapturado.Resposta,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(respostaDesserializada);
        Assert.Equal(dto.CodigoDoCondominio, respostaDesserializada!.CodigoDoCondominio);
        Assert.Equal(dto.NomeDoCondominio, respostaDesserializada.NomeDoCondominio);

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
            chaveCache: "GET:/api/condominios",
            urlDaConsulta: "/api/condominios?nomeDoCondominio=residencial&pagina=1",
            tipoConsulta: "CONDOMINIO_NOME",
            entidade: "condominios",
            entidadeId: null,
            resposta: new ResultadoPaginadoDto<InformacoesCondominioDto>(),
            statusCode: 200);

        Assert.NotNull(cacheEntryCapturado);
        Assert.Equal(200, cacheEntryCapturado!.StatusCode);
        Assert.Equal("CONDOMINIO_NOME", cacheEntryCapturado.TipoConsulta);
        Assert.Null(cacheEntryCapturado.EntidadeId);

        repository.Verify(
            x => x.SalvarAsync(It.IsAny<CacheEntry>()),
            Times.Once);
    }

    private static InformacoesCondominioDto CreateDto()
    {
        return new InformacoesCondominioDto
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