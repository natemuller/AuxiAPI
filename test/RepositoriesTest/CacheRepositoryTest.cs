using System.Text.Json;
using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.Tests.RepositoriesTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class CacheRepositoryTest(PostgresTestFixture fixture) : IAsyncLifetime
{
    private readonly PostgresTestFixture _fixture = fixture;
    private CondominiosDbContext _context = null!;
    private CacheRepository _repository = null!;

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        _context = new CondominiosDbContext(new DbContextOptionsBuilder<CondominiosDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options);

        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        _repository = new CacheRepository(_context);
    }

    public async Task DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.DisposeAsync();
        }
    }

    [Fact]
    public async Task ObterValidoPorChaveAsync_DeveRetornarCacheValido()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var cacheEntry = CreateCacheEntry(
            chaveCache: "GET:/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396,
            criadoEm: DateTime.UtcNow.AddMinutes(-1),
            expiradoEm: DateTime.UtcNow.AddMinutes(14),
            invalidadoEm: null);

        await SeedAsync(cacheEntry);

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/5396");

        Assert.NotNull(resultado);
        Assert.Equal(cacheEntry.Id, resultado!.Id);
        Assert.Equal("GET:/api/condominios/5396", resultado.ChaveCache);
        Assert.Equal("CONDOMINIO_CODCONDOM", resultado.TipoConsulta);
        Assert.Equal("atlas_condominios", resultado.Entidade);
        Assert.Equal(5396, resultado.EntidadeId);
        Assert.Null(resultado.InvalidadoEm);
        Assert.True(resultado.ExpiradoEm > DateTime.UtcNow);
    }

    [Fact]
    public async Task ObterValidoPorChaveAsync_DeveRetornarNull_QuandoCacheEstiverExpirado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var cacheEntry = CreateCacheEntry(
            chaveCache: "GET:/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396,
            criadoEm: DateTime.UtcNow.AddMinutes(-20),
            expiradoEm: DateTime.UtcNow.AddMinutes(-5),
            invalidadoEm: null);

        await SeedAsync(cacheEntry);

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/5396");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterValidoPorChaveAsync_DeveRetornarNull_QuandoCacheEstiverInvalidado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var cacheEntry = CreateCacheEntry(
            chaveCache: "GET:/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396,
            criadoEm: DateTime.UtcNow.AddMinutes(-1),
            expiradoEm: DateTime.UtcNow.AddMinutes(14),
            invalidadoEm: DateTime.UtcNow);

        await SeedAsync(cacheEntry);

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/5396");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterValidoPorChaveAsync_DeveRetornarNull_QuandoChaveNaoExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateCacheEntry(
            chaveCache: "GET:/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396,
            criadoEm: DateTime.UtcNow.AddMinutes(-1),
            expiradoEm: DateTime.UtcNow.AddMinutes(14),
            invalidadoEm: null));

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/9999");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterValidoPorChaveAsync_DeveRetornarCacheMaisRecente_QuandoHouverMaisDeUmValido()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var cacheAntigo = CreateCacheEntry(
            chaveCache: "GET:/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396,
            criadoEm: DateTime.UtcNow.AddMinutes(-10),
            expiradoEm: DateTime.UtcNow.AddMinutes(5),
            invalidadoEm: null,
            nomeCondominio: "Cache Antigo");

        var cacheNovo = CreateCacheEntry(
            chaveCache: "GET:/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396,
            criadoEm: DateTime.UtcNow.AddMinutes(-1),
            expiradoEm: DateTime.UtcNow.AddMinutes(14),
            invalidadoEm: null,
            nomeCondominio: "Cache Novo");

        await SeedAsync(cacheAntigo, cacheNovo);

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/5396");

        Assert.NotNull(resultado);
        Assert.Equal(cacheNovo.Id, resultado!.Id);
        Assert.Contains("Cache Novo", resultado.Resposta);
    }

    [Fact]
    public async Task SalvarAsync_DevePersistirCacheEntryPorCodCondom()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var cacheEntry = CreateCacheEntry(
            chaveCache: "GET:/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396,
            criadoEm: DateTime.UtcNow,
            expiradoEm: DateTime.UtcNow.AddMinutes(15),
            invalidadoEm: null);

        await _repository.SalvarAsync(cacheEntry);

        var registroSalvo = await _context.Cache
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cacheEntry.Id);

        Assert.NotNull(registroSalvo);
        Assert.Equal("GET:/api/condominios/5396", registroSalvo!.ChaveCache);
        Assert.Equal("/api/condominios/5396", registroSalvo.UrlDaConsulta);
        Assert.Equal("CONDOMINIO_CODCONDOM", registroSalvo.TipoConsulta);
        Assert.Equal("atlas_condominios", registroSalvo.Entidade);
        Assert.Equal(5396, registroSalvo.EntidadeId);
        Assert.Equal(200, registroSalvo.StatusCode);
        Assert.Null(registroSalvo.InvalidadoEm);
    }

    [Fact]
    public async Task SalvarAsync_DevePersistirCacheEntryPorCnpjComEntidadeIdNull()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var cacheEntry = CreateCacheEntry(
            chaveCache: "GET:/api/condominios?cnpj=17474690000113&pagina=1",
            urlDaConsulta: "/api/condominios?cnpj=17474690000113&pagina=1",
            tipoConsulta: "CONDOMINIO_CNPJ",
            entidadeId: null,
            criadoEm: DateTime.UtcNow,
            expiradoEm: DateTime.UtcNow.AddMinutes(15),
            invalidadoEm: null);

        await _repository.SalvarAsync(cacheEntry);

        var registroSalvo = await _context.Cache
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cacheEntry.Id);

        Assert.NotNull(registroSalvo);
        Assert.Equal("GET:/api/condominios?cnpj=17474690000113&pagina=1", registroSalvo!.ChaveCache);
        Assert.Equal("/api/condominios?cnpj=17474690000113&pagina=1", registroSalvo.UrlDaConsulta);
        Assert.Equal("CONDOMINIO_CNPJ", registroSalvo.TipoConsulta);
        Assert.Equal("atlas_condominios", registroSalvo.Entidade);
        Assert.Null(registroSalvo.EntidadeId);
        Assert.Equal(200, registroSalvo.StatusCode);
        Assert.Null(registroSalvo.InvalidadoEm);
    }

    private async Task SeedAsync(params CacheEntry[] cacheEntries)
    {
        _context.Cache.AddRange(cacheEntries);
        await _context.SaveChangesAsync();
    }

    private static CacheEntry CreateCacheEntry(
        string chaveCache,
        string urlDaConsulta,
        string tipoConsulta,
        int? entidadeId,
        DateTime criadoEm,
        DateTime expiradoEm,
        DateTime? invalidadoEm,
        string nomeCondominio = "SOLAR DI TOSCANA")
    {
        var dto = new AtlasCondominioDto
        {
            CodCondom = 5396,
            NomeCondom = nomeCondominio,
            Ativo = "S",
            Cnpj = "17474690000113",
            QtdBlocos = 1,
            QtdUnidades = 9,
            Cidade = "Porto Alegre",
            Uf = "RS"
        };

        return new CacheEntry
        {
            Id = Guid.NewGuid(),
            ChaveCache = chaveCache,
            UrlDaConsulta = urlDaConsulta,
            MetodoHttp = "GET",
            TipoConsulta = tipoConsulta,
            Entidade = "atlas_condominios",
            EntidadeId = entidadeId,
            Resposta = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            StatusCode = 200,
            CriadoEm = criadoEm,
            ExpiradoEm = expiradoEm,
            InvalidadoEm = invalidadoEm,
            MotivoInvalidacao = invalidadoEm is null ? null : "Teste de invalidação"
        };
    }
}