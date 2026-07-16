using AuxiAPI.src.Contexts;
using AuxiAPI.src.Entities;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.Tests.IntegrationTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class UnidadesCacheInvalidationTriggerTest(PostgresTestFixture fixture) : IAsyncLifetime
{
    private readonly PostgresTestFixture _fixture = fixture;
    private CondominiosDbContext _context = null!;

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        _context = new CondominiosDbContext(new DbContextOptionsBuilder<CondominiosDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options);

        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        await RemoverForeignKeysDeAtlasUnidadesAsync();
        await CriarTriggerInvalidacaoAtlasUnidadesAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context is not null)
            await _context.DisposeAsync();
    }

    [Fact]
    public async Task UpdateEmAtlasUnidades_DeveInvalidarCachePorIdEconomiaECachesDeListagem()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedUnidadesAsync(
            CriarUnidade(1, 5396, "João Silva"));

        await SeedCacheAsync(
            CriarCache(
                chaveCache: "GET:/api/unidades/1",
                tipoConsulta: "UNIDADE_IDECONOMIA",
                entidade: "atlas_unidades",
                entidadeId: 1),
            CriarCache(
                chaveCache: "GET:/api/unidades?codCondom=5396&pagina=1",
                tipoConsulta: "UNIDADE_CODCONDOM",
                entidade: "atlas_unidades",
                entidadeId: null),
            CriarCache(
                chaveCache: "GET:/api/unidades?nomeCondomino=joao&pagina=1",
                tipoConsulta: "UNIDADE_NOME_CONDOMINO",
                entidade: "atlas_unidades",
                entidadeId: null),
            CriarCache(
                chaveCache: "GET:/api/condominios/5396",
                tipoConsulta: "CONDOMINIO_CODCONDOM",
                entidade: "atlas_condominios",
                entidadeId: 5396));

        await _context.Database.ExecuteSqlRawAsync("""
            update public.atlas_unidades
            set nome_condomino = 'João Silva Atualizado'
            where ideconomia = 1;
            """);

        _context.ChangeTracker.Clear();

        var cacheIdEconomia = await BuscarCacheAsync("GET:/api/unidades/1");
        var cacheCodCondom = await BuscarCacheAsync("GET:/api/unidades?codCondom=5396&pagina=1");
        var cacheNomeCondomino = await BuscarCacheAsync("GET:/api/unidades?nomeCondomino=joao&pagina=1");
        var cacheCondominio = await BuscarCacheAsync("GET:/api/condominios/5396");

        Assert.NotNull(cacheIdEconomia.InvalidadoEm);
        Assert.NotNull(cacheCodCondom.InvalidadoEm);
        Assert.NotNull(cacheNomeCondomino.InvalidadoEm);

        Assert.Equal("Registro da tabela atlas_unidades alterado", cacheIdEconomia.MotivoInvalidacao);
        Assert.Equal("Registro da tabela atlas_unidades alterado", cacheCodCondom.MotivoInvalidacao);
        Assert.Equal("Registro da tabela atlas_unidades alterado", cacheNomeCondomino.MotivoInvalidacao);

        Assert.Null(cacheCondominio.InvalidadoEm);
        Assert.Null(cacheCondominio.MotivoInvalidacao);
    }

    [Fact]
    public async Task UpdateEmAtlasUnidades_NaoDeveInvalidarCachePorIdEconomiaDeOutraUnidade()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedUnidadesAsync(
            CriarUnidade(1, 5396, "João Silva"),
            CriarUnidade(2, 5396, "Maria Souza"));

        await SeedCacheAsync(
            CriarCache(
                chaveCache: "GET:/api/unidades/1",
                tipoConsulta: "UNIDADE_IDECONOMIA",
                entidade: "atlas_unidades",
                entidadeId: 1),
            CriarCache(
                chaveCache: "GET:/api/unidades/2",
                tipoConsulta: "UNIDADE_IDECONOMIA",
                entidade: "atlas_unidades",
                entidadeId: 2));

        await _context.Database.ExecuteSqlRawAsync("""
            update public.atlas_unidades
            set nome_condomino = 'Maria Souza Atualizada'
            where ideconomia = 2;
            """);

        _context.ChangeTracker.Clear();

        var cacheUnidade1 = await BuscarCacheAsync("GET:/api/unidades/1");
        var cacheUnidade2 = await BuscarCacheAsync("GET:/api/unidades/2");

        Assert.Null(cacheUnidade1.InvalidadoEm);
        Assert.Null(cacheUnidade1.MotivoInvalidacao);

        Assert.NotNull(cacheUnidade2.InvalidadoEm);
        Assert.Equal("Registro da tabela atlas_unidades alterado", cacheUnidade2.MotivoInvalidacao);
    }

    [Fact]
    public async Task DeleteEmAtlasUnidades_DeveInvalidarCachePorIdEconomia()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedUnidadesAsync(
            CriarUnidade(1, 5396, "João Silva"));

        await SeedCacheAsync(
            CriarCache(
                chaveCache: "GET:/api/unidades/1",
                tipoConsulta: "UNIDADE_IDECONOMIA",
                entidade: "atlas_unidades",
                entidadeId: 1));

        await _context.Database.ExecuteSqlRawAsync("""
            delete from public.atlas_unidades
            where ideconomia = 1;
            """);

        _context.ChangeTracker.Clear();

        var cache = await BuscarCacheAsync("GET:/api/unidades/1");

        Assert.NotNull(cache.InvalidadoEm);
        Assert.Equal("Registro da tabela atlas_unidades alterado", cache.MotivoInvalidacao);
    }

    [Fact]
    public async Task InsertEmAtlasUnidades_DeveInvalidarCachesDeListagem()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedCacheAsync(
            CriarCache(
                chaveCache: "GET:/api/unidades?codCondom=5396&pagina=1",
                tipoConsulta: "UNIDADE_CODCONDOM",
                entidade: "atlas_unidades",
                entidadeId: null),
            CriarCache(
                chaveCache: "GET:/api/unidades?nomeCondomino=joao&pagina=1",
                tipoConsulta: "UNIDADE_NOME_CONDOMINO",
                entidade: "atlas_unidades",
                entidadeId: null));

        await SeedUnidadesAsync(
            CriarUnidade(1, 5396, "João Silva"));

        _context.ChangeTracker.Clear();

        var cacheCodCondom = await BuscarCacheAsync("GET:/api/unidades?codCondom=5396&pagina=1");
        var cacheNomeCondomino = await BuscarCacheAsync("GET:/api/unidades?nomeCondomino=joao&pagina=1");

        Assert.NotNull(cacheCodCondom.InvalidadoEm);
        Assert.NotNull(cacheNomeCondomino.InvalidadoEm);

        Assert.Equal("Registro da tabela atlas_unidades alterado", cacheCodCondom.MotivoInvalidacao);
        Assert.Equal("Registro da tabela atlas_unidades alterado", cacheNomeCondomino.MotivoInvalidacao);
    }

    private async Task<CacheEntry> BuscarCacheAsync(string chaveCache)
    {
        return await _context.Cache
            .AsNoTracking()
            .SingleAsync(c => c.ChaveCache == chaveCache);
    }

    private async Task SeedUnidadesAsync(params AtlasUnidade[] unidades)
    {
        _context.AtlasUnidades.AddRange(unidades);
        await _context.SaveChangesAsync();
    }

    private async Task SeedCacheAsync(params CacheEntry[] caches)
    {
        _context.Cache.AddRange(caches);
        await _context.SaveChangesAsync();
    }

    private static AtlasUnidade CriarUnidade(
        int idEconomia,
        int codCondom,
        string nomeCondomino)
    {
        return new AtlasUnidade
        {
            IdEconomia = idEconomia,
            CodCondom = codCondom,
            CodBloco = "A",
            CodEconom = "101",
            Fracao = 0.08280000m,
            Ativa = "S",
            DataDesativa = null,
            DtAlteracao = new DateTime(2026, 7, 16),
            TipoUnidade = "Apartamento",
            CodCondomino = idEconomia.ToString(),
            NomeCondomino = nomeCondomino,
            EnderecoPrincipal = "Rua Teste, 123",
            EnderecoCorrespondencia = null,
            EnderecoCobranca = null,
            CodPesDebConta = null,
            NomeDebConta = null,
            CodFornec = null,
            CodNaAdmDest = null,
            CodFornecEscrit = null
        };
    }

    private static CacheEntry CriarCache(
        string chaveCache,
        string tipoConsulta,
        string entidade,
        int? entidadeId)
    {
        var agora = DateTime.UtcNow;

        return new CacheEntry
        {
            Id = Guid.NewGuid(),
            ChaveCache = chaveCache,
            UrlDaConsulta = chaveCache.Replace("GET:", string.Empty),
            MetodoHttp = "GET",
            TipoConsulta = tipoConsulta,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Resposta = "{}",
            StatusCode = 200,
            CriadoEm = agora,
            ExpiradoEm = agora.AddMinutes(15),
            InvalidadoEm = null,
            MotivoInvalidacao = null
        };
    }

    private async Task RemoverForeignKeysDeAtlasUnidadesAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            do $$
            declare
                constraint_atual record;
            begin
                for constraint_atual in
                    select conname
                    from pg_constraint
                    where conrelid = 'public.atlas_unidades'::regclass
                      and contype = 'f'
                loop
                    execute format(
                        'alter table public.atlas_unidades drop constraint %I',
                        constraint_atual.conname
                    );
                end loop;
            end $$;
            """);
    }

    private async Task CriarTriggerInvalidacaoAtlasUnidadesAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            create or replace function public.invalidar_cache_atlas_unidades()
            returns trigger
            language plpgsql
            as $$
            declare
                id_economia_atual integer;
                id_economia_antigo integer;
            begin
                if (TG_OP = 'DELETE') then
                    id_economia_atual := OLD.ideconomia;
                    id_economia_antigo := OLD.ideconomia;
                elsif (TG_OP = 'UPDATE') then
                    id_economia_atual := NEW.ideconomia;
                    id_economia_antigo := OLD.ideconomia;
                else
                    id_economia_atual := NEW.ideconomia;
                    id_economia_antigo := NEW.ideconomia;
                end if;

                update public.cache
                set
                    invalidado_em = now(),
                    motivo_invalidacao = 'Registro da tabela atlas_unidades alterado'
                where
                    entidade = 'atlas_unidades'
                    and invalidado_em is null
                    and expirado_em > now()
                    and tipo_consulta = 'UNIDADE_IDECONOMIA'
                    and entidade_id = id_economia_atual;

                if (TG_OP = 'UPDATE' and id_economia_antigo <> id_economia_atual) then
                    update public.cache
                    set
                        invalidado_em = now(),
                        motivo_invalidacao = 'Registro da tabela atlas_unidades alterado'
                    where
                        entidade = 'atlas_unidades'
                        and invalidado_em is null
                        and expirado_em > now()
                        and tipo_consulta = 'UNIDADE_IDECONOMIA'
                        and entidade_id = id_economia_antigo;
                end if;

                update public.cache
                set
                    invalidado_em = now(),
                    motivo_invalidacao = 'Registro da tabela atlas_unidades alterado'
                where
                    entidade = 'atlas_unidades'
                    and invalidado_em is null
                    and expirado_em > now()
                    and tipo_consulta in (
                        'UNIDADE_CODCONDOM',
                        'UNIDADE_NOME_CONDOMINO'
                    );

                if (TG_OP = 'DELETE') then
                    return OLD;
                end if;

                return NEW;
            end;
            $$;

            drop trigger if exists trg_invalidar_cache_atlas_unidades
            on public.atlas_unidades;

            create trigger trg_invalidar_cache_atlas_unidades
            after insert or update or delete
            on public.atlas_unidades
            for each row
            execute function public.invalidar_cache_atlas_unidades();
            """);
    }
}