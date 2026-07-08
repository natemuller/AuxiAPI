using AuxiAPI.src.Contexts;
using AuxiAPI.src.Entities;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.Tests.IntegrationTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class CacheInvalidationTriggerTest(PostgresTestFixture fixture) : IAsyncLifetime
{
    private readonly PostgresTestFixture _fixture = fixture;
    private CondominiosDbContext _context = null!;

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

        // Importante: MigrateAsync aplica as migrations, incluindo a trigger.
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.DisposeAsync();
        }
    }

    [Fact]
    public async Task Trigger_DeveInvalidarCachePorIdEPorNome_QuandoCondominioForAtualizado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await LimparTabelasAsync();

        var condominio = CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa");

        _context.Condominios.Add(condominio);
        await _context.SaveChangesAsync();

        var cachePorId = CreateCacheEntry(
            chaveCache: $"GET:/api/condominios/{condominio.Id}",
            urlDaConsulta: $"/api/condominios/{condominio.Id}",
            tipoConsulta: "CONDOMINIO_ID",
            entidadeId: condominio.Id);

        var cachePorNome = CreateCacheEntry(
            chaveCache: "GET:/api/condominios?nomeDoCondominio=residencial&pagina=1",
            urlDaConsulta: "/api/condominios?nomeDoCondominio=residencial&pagina=1",
            tipoConsulta: "CONDOMINIO_NOME",
            entidadeId: null);

        _context.Cache.AddRange(cachePorId, cachePorNome);
        await _context.SaveChangesAsync();

        condominio.NomeDoCondominio = "Residencial Nome Alterado";
        await _context.SaveChangesAsync();

        var cachePorIdAtualizado = await _context.Cache
            .AsNoTracking()
            .FirstAsync(c => c.Id == cachePorId.Id);

        var cachePorNomeAtualizado = await _context.Cache
            .AsNoTracking()
            .FirstAsync(c => c.Id == cachePorNome.Id);

        Assert.NotNull(cachePorIdAtualizado.InvalidadoEm);
        Assert.NotNull(cachePorNomeAtualizado.InvalidadoEm);

        Assert.Equal(
            "Registro da tabela condominios alterado",
            cachePorIdAtualizado.MotivoInvalidacao);

        Assert.Equal(
            "Registro da tabela condominios alterado",
            cachePorNomeAtualizado.MotivoInvalidacao);
    }

    [Fact]
    public async Task Trigger_DeveInvalidarCachePorNome_QuandoCondominioForInserido()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await LimparTabelasAsync();

        var cachePorNome = CreateCacheEntry(
            chaveCache: "GET:/api/condominios?nomeDoCondominio=residencial&pagina=1",
            urlDaConsulta: "/api/condominios?nomeDoCondominio=residencial&pagina=1",
            tipoConsulta: "CONDOMINIO_NOME",
            entidadeId: null);

        _context.Cache.Add(cachePorNome);
        await _context.SaveChangesAsync();

        _context.Condominios.Add(
            CreateCondominio("0001", "12345678000101", "Residencial Novo"));

        await _context.SaveChangesAsync();

        var cacheAtualizado = await _context.Cache
            .AsNoTracking()
            .FirstAsync(c => c.Id == cachePorNome.Id);

        Assert.NotNull(cacheAtualizado.InvalidadoEm);

        Assert.Equal(
            "Registro da tabela condominios alterado",
            cacheAtualizado.MotivoInvalidacao);
    }

    [Fact]
    public async Task Trigger_DeveInvalidarCachePorIdEPorNome_QuandoCondominioForDeletado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await LimparTabelasAsync();

        var condominio = CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa");

        _context.Condominios.Add(condominio);
        await _context.SaveChangesAsync();

        var cachePorId = CreateCacheEntry(
            chaveCache: $"GET:/api/condominios/{condominio.Id}",
            urlDaConsulta: $"/api/condominios/{condominio.Id}",
            tipoConsulta: "CONDOMINIO_ID",
            entidadeId: condominio.Id);

        var cachePorNome = CreateCacheEntry(
            chaveCache: "GET:/api/condominios?nomeDoCondominio=residencial&pagina=1",
            urlDaConsulta: "/api/condominios?nomeDoCondominio=residencial&pagina=1",
            tipoConsulta: "CONDOMINIO_NOME",
            entidadeId: null);

        _context.Cache.AddRange(cachePorId, cachePorNome);
        await _context.SaveChangesAsync();

        _context.Condominios.Remove(condominio);
        await _context.SaveChangesAsync();

        var cachePorIdAtualizado = await _context.Cache
            .AsNoTracking()
            .FirstAsync(c => c.Id == cachePorId.Id);

        var cachePorNomeAtualizado = await _context.Cache
            .AsNoTracking()
            .FirstAsync(c => c.Id == cachePorNome.Id);

        Assert.NotNull(cachePorIdAtualizado.InvalidadoEm);
        Assert.NotNull(cachePorNomeAtualizado.InvalidadoEm);
    }

    private async Task LimparTabelasAsync()
    {
        _context.Cache.RemoveRange(_context.Cache);
        _context.Condominios.RemoveRange(_context.Condominios);

        await _context.SaveChangesAsync();
    }

    private static CacheEntry CreateCacheEntry(
        string chaveCache,
        string urlDaConsulta,
        string tipoConsulta,
        int? entidadeId)
    {
        var agora = DateTime.UtcNow;

        return new CacheEntry
        {
            Id = Guid.NewGuid(),
            ChaveCache = chaveCache,
            UrlDaConsulta = urlDaConsulta,
            MetodoHttp = "GET",
            TipoConsulta = tipoConsulta,
            Entidade = "condominios",
            EntidadeId = entidadeId,
            Resposta = "{}",
            StatusCode = 200,
            CriadoEm = agora,
            ExpiradoEm = agora.AddMinutes(15),
            InvalidadoEm = null,
            MotivoInvalidacao = null
        };
    }

    private static Condominio CreateCondominio(string codigo, string cnpj, string nome)
    {
        return new Condominio
        {
            CodigoDoCondominio = codigo,
            CNPJDoCondominio = cnpj,
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
}