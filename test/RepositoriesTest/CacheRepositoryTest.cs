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
            chaveCache: "GET:/api/condominios/1",
            criadoEm: DateTime.UtcNow.AddMinutes(-1),
            expiradoEm: DateTime.UtcNow.AddMinutes(14),
            invalidadoEm: null);

        await SeedAsync(cacheEntry);

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/1");

        Assert.NotNull(resultado);
        Assert.Equal(cacheEntry.Id, resultado!.Id);
        Assert.Equal("GET:/api/condominios/1", resultado.ChaveCache);
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
            chaveCache: "GET:/api/condominios/1",
            criadoEm: DateTime.UtcNow.AddMinutes(-20),
            expiradoEm: DateTime.UtcNow.AddMinutes(-5),
            invalidadoEm: null);

        await SeedAsync(cacheEntry);

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/1");

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
            chaveCache: "GET:/api/condominios/1",
            criadoEm: DateTime.UtcNow.AddMinutes(-1),
            expiradoEm: DateTime.UtcNow.AddMinutes(14),
            invalidadoEm: DateTime.UtcNow);

        await SeedAsync(cacheEntry);

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/1");

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
            chaveCache: "GET:/api/condominios/1",
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
            chaveCache: "GET:/api/condominios/1",
            criadoEm: DateTime.UtcNow.AddMinutes(-10),
            expiradoEm: DateTime.UtcNow.AddMinutes(5),
            invalidadoEm: null,
            nomeCondominio: "Cache Antigo");

        var cacheNovo = CreateCacheEntry(
            chaveCache: "GET:/api/condominios/1",
            criadoEm: DateTime.UtcNow.AddMinutes(-1),
            expiradoEm: DateTime.UtcNow.AddMinutes(14),
            invalidadoEm: null,
            nomeCondominio: "Cache Novo");

        await SeedAsync(cacheAntigo, cacheNovo);

        var resultado = await _repository.ObterValidoPorChaveAsync("GET:/api/condominios/1");

        Assert.NotNull(resultado);
        Assert.Equal(cacheNovo.Id, resultado!.Id);
        Assert.Contains("Cache Novo", resultado.Resposta);
    }

    [Fact]
    public async Task SalvarAsync_DevePersistirCacheEntry()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var cacheEntry = CreateCacheEntry(
            chaveCache: "GET:/api/condominios/1",
            criadoEm: DateTime.UtcNow,
            expiradoEm: DateTime.UtcNow.AddMinutes(15),
            invalidadoEm: null);

        await _repository.SalvarAsync(cacheEntry);

        var registroSalvo = await _context.Cache
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cacheEntry.Id);

        Assert.NotNull(registroSalvo);
        Assert.Equal("GET:/api/condominios/1", registroSalvo!.ChaveCache);
        Assert.Equal("/api/condominios/1", registroSalvo.UrlDaConsulta);
        Assert.Equal("CONDOMINIO_ID", registroSalvo.TipoConsulta);
        Assert.Equal("condominios", registroSalvo.Entidade);
        Assert.Equal(1, registroSalvo.EntidadeId);
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
        DateTime criadoEm,
        DateTime expiradoEm,
        DateTime? invalidadoEm,
        string nomeCondominio = "Residencial Brasil-Hexa")
    {
        var dto = new InformacoesCondominioDto
        {
            CodigoDoCondominio = "0001",
            CNPJDoCondominio = "12345678000101",
            NomeDoCondominio = nomeCondominio,
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

        return new CacheEntry
        {
            Id = Guid.NewGuid(),
            ChaveCache = chaveCache,
            UrlDaConsulta = "/api/condominios/1",
            MetodoHttp = "GET",
            TipoConsulta = "CONDOMINIO_ID",
            Entidade = "condominios",
            EntidadeId = 1,
            Resposta = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            StatusCode = 200,
            CriadoEm = criadoEm,
            ExpiradoEm = expiradoEm,
            InvalidadoEm = invalidadoEm,
            MotivoInvalidacao = invalidadoEm is null ? null : "Teste de invalidação"
        };
    }
}