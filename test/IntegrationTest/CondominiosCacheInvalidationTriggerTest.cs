using AuxiAPI.src.Contexts;
using AuxiAPI.src.Entities;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.Tests.IntegrationTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class CondominiosCacheInvalidationTriggerTest(PostgresTestFixture fixture) : IAsyncLifetime
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
        await _context.Database.EnsureCreatedAsync();

        await CriarTriggerDeInvalidacaoAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.DisposeAsync();
        }
    }

    [Fact]
    public async Task Trigger_DeveInvalidarCachePorCodCondomNomeECnpj_QuandoAtlasCondominioForAtualizado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await LimparTabelasAsync();

        var condominio = CreateAtlasCondominio(
            codCondom: 5396,
            cnpj: "17474690000113",
            nome: "SOLAR DI TOSCANA");

        _context.AtlasCondominios.Add(condominio);
        await _context.SaveChangesAsync();

        var cachePorCodCondom = CreateCacheEntry(
            chaveCache: "/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396);

        var cachePorNome = CreateCacheEntry(
            chaveCache: "/api/condominios?nomeCondom=solar&pagina=1",
            urlDaConsulta: "/api/condominios?nomeCondom=solar&pagina=1",
            tipoConsulta: "CONDOMINIO_NOME",
            entidadeId: null);

        var cachePorCnpj = CreateCacheEntry(
            chaveCache: "/api/condominios?cnpj=17474690000113&pagina=1",
            urlDaConsulta: "/api/condominios?cnpj=17474690000113&pagina=1",
            tipoConsulta: "CONDOMINIO_CNPJ",
            entidadeId: null);

        _context.Cache.AddRange(cachePorCodCondom, cachePorNome, cachePorCnpj);
        await _context.SaveChangesAsync();

        condominio.NomeCondom = "SOLAR DI TOSCANA ALTERADO";
        condominio.DtAlteracao = new DateTime(2026, 7, 14, 11, 0, 0);

        await _context.SaveChangesAsync();

        var cachePorCodCondomAtualizado = await BuscarCacheAsync(cachePorCodCondom.Id);
        var cachePorNomeAtualizado = await BuscarCacheAsync(cachePorNome.Id);
        var cachePorCnpjAtualizado = await BuscarCacheAsync(cachePorCnpj.Id);

        Assert.NotNull(cachePorCodCondomAtualizado.InvalidadoEm);
        Assert.NotNull(cachePorNomeAtualizado.InvalidadoEm);
        Assert.NotNull(cachePorCnpjAtualizado.InvalidadoEm);

        Assert.Equal(
            "Registro da tabela atlas_condominios alterado",
            cachePorCodCondomAtualizado.MotivoInvalidacao);

        Assert.Equal(
            "Registro da tabela atlas_condominios alterado",
            cachePorNomeAtualizado.MotivoInvalidacao);

        Assert.Equal(
            "Registro da tabela atlas_condominios alterado",
            cachePorCnpjAtualizado.MotivoInvalidacao);
    }

    [Fact]
    public async Task Trigger_DeveInvalidarCachePorNomeECnpj_QuandoAtlasCondominioForInserido()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await LimparTabelasAsync();

        var cachePorNome = CreateCacheEntry(
            chaveCache: "/api/condominios?nomeCondom=solar&pagina=1",
            urlDaConsulta: "/api/condominios?nomeCondom=solar&pagina=1",
            tipoConsulta: "CONDOMINIO_NOME",
            entidadeId: null);

        var cachePorCnpj = CreateCacheEntry(
            chaveCache: "/api/condominios?cnpj=17474690000113&pagina=1",
            urlDaConsulta: "/api/condominios?cnpj=17474690000113&pagina=1",
            tipoConsulta: "CONDOMINIO_CNPJ",
            entidadeId: null);

        _context.Cache.AddRange(cachePorNome, cachePorCnpj);
        await _context.SaveChangesAsync();

        _context.AtlasCondominios.Add(
            CreateAtlasCondominio(
                codCondom: 5396,
                cnpj: "17474690000113",
                nome: "SOLAR DI TOSCANA"));

        await _context.SaveChangesAsync();

        var cachePorNomeAtualizado = await BuscarCacheAsync(cachePorNome.Id);
        var cachePorCnpjAtualizado = await BuscarCacheAsync(cachePorCnpj.Id);

        Assert.NotNull(cachePorNomeAtualizado.InvalidadoEm);
        Assert.NotNull(cachePorCnpjAtualizado.InvalidadoEm);

        Assert.Equal(
            "Registro da tabela atlas_condominios alterado",
            cachePorNomeAtualizado.MotivoInvalidacao);

        Assert.Equal(
            "Registro da tabela atlas_condominios alterado",
            cachePorCnpjAtualizado.MotivoInvalidacao);
    }

    [Fact]
    public async Task Trigger_DeveInvalidarCachePorCodCondomNomeECnpj_QuandoAtlasCondominioForDeletado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await LimparTabelasAsync();

        var condominio = CreateAtlasCondominio(
            codCondom: 5396,
            cnpj: "17474690000113",
            nome: "SOLAR DI TOSCANA");

        _context.AtlasCondominios.Add(condominio);
        await _context.SaveChangesAsync();

        var cachePorCodCondom = CreateCacheEntry(
            chaveCache: "/api/condominios/5396",
            urlDaConsulta: "/api/condominios/5396",
            tipoConsulta: "CONDOMINIO_CODCONDOM",
            entidadeId: 5396);

        var cachePorNome = CreateCacheEntry(
            chaveCache: "/api/condominios?nomeCondom=solar&pagina=1",
            urlDaConsulta: "/api/condominios?nomeCondom=solar&pagina=1",
            tipoConsulta: "CONDOMINIO_NOME",
            entidadeId: null);

        var cachePorCnpj = CreateCacheEntry(
            chaveCache: "/api/condominios?cnpj=17474690000113&pagina=1",
            urlDaConsulta: "/api/condominios?cnpj=17474690000113&pagina=1",
            tipoConsulta: "CONDOMINIO_CNPJ",
            entidadeId: null);

        _context.Cache.AddRange(cachePorCodCondom, cachePorNome, cachePorCnpj);
        await _context.SaveChangesAsync();

        _context.AtlasCondominios.Remove(condominio);
        await _context.SaveChangesAsync();

        var cachePorCodCondomAtualizado = await BuscarCacheAsync(cachePorCodCondom.Id);
        var cachePorNomeAtualizado = await BuscarCacheAsync(cachePorNome.Id);
        var cachePorCnpjAtualizado = await BuscarCacheAsync(cachePorCnpj.Id);

        Assert.NotNull(cachePorCodCondomAtualizado.InvalidadoEm);
        Assert.NotNull(cachePorNomeAtualizado.InvalidadoEm);
        Assert.NotNull(cachePorCnpjAtualizado.InvalidadoEm);
    }

    private async Task<CacheEntry> BuscarCacheAsync(Guid id)
    {
        return await _context.Cache
            .AsNoTracking()
            .FirstAsync(c => c.Id == id);
    }

    private async Task LimparTabelasAsync()
    {
        _context.Cache.RemoveRange(_context.Cache);
        _context.AtlasCondominios.RemoveRange(_context.AtlasCondominios);

        await _context.SaveChangesAsync();
    }

    private async Task CriarTriggerDeInvalidacaoAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            create or replace function public.invalidar_cache_atlas_condominios()
            returns trigger
            language plpgsql
            as $$
            declare
                cod_condom_atual integer;
                cod_condom_antigo integer;
            begin
                if (TG_OP = 'DELETE') then
                    cod_condom_atual := OLD.codcondom;
                    cod_condom_antigo := OLD.codcondom;
                elsif (TG_OP = 'UPDATE') then
                    cod_condom_atual := NEW.codcondom;
                    cod_condom_antigo := OLD.codcondom;
                else
                    cod_condom_atual := NEW.codcondom;
                    cod_condom_antigo := NEW.codcondom;
                end if;

                update public.cache
                set
                    invalidado_em = now(),
                    motivo_invalidacao = 'Registro da tabela atlas_condominios alterado'
                where
                    entidade = 'atlas_condominios'
                    and invalidado_em is null
                    and expirado_em > now()
                    and tipo_consulta = 'CONDOMINIO_CODCONDOM'
                    and entidade_id = cod_condom_atual;

                if (TG_OP = 'UPDATE' and cod_condom_antigo <> cod_condom_atual) then
                    update public.cache
                    set
                        invalidado_em = now(),
                        motivo_invalidacao = 'Registro da tabela atlas_condominios alterado'
                    where
                        entidade = 'atlas_condominios'
                        and invalidado_em is null
                        and expirado_em > now()
                        and tipo_consulta = 'CONDOMINIO_CODCONDOM'
                        and entidade_id = cod_condom_antigo;
                end if;

                update public.cache
                set
                    invalidado_em = now(),
                    motivo_invalidacao = 'Registro da tabela atlas_condominios alterado'
                where
                    entidade = 'atlas_condominios'
                    and invalidado_em is null
                    and expirado_em > now()
                    and tipo_consulta in (
                        'CONDOMINIO_NOME',
                        'CONDOMINIO_CNPJ'
                    );

                if (TG_OP = 'DELETE') then
                    return OLD;
                end if;

                return NEW;
            end;
            $$;

            drop trigger if exists trg_invalidar_cache_atlas_condominios
            on public.atlas_condominios;

            create trigger trg_invalidar_cache_atlas_condominios
            after insert or update or delete
            on public.atlas_condominios
            for each row
            execute function public.invalidar_cache_atlas_condominios();
            """);
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
            ChaveCache = $"GET:{chaveCache}",
            UrlDaConsulta = urlDaConsulta,
            MetodoHttp = "GET",
            TipoConsulta = tipoConsulta,
            Entidade = "atlas_condominios",
            EntidadeId = entidadeId,
            Resposta = "{}",
            StatusCode = 200,
            CriadoEm = agora,
            ExpiradoEm = agora.AddMinutes(15),
            InvalidadoEm = null,
            MotivoInvalidacao = null
        };
    }

    private static AtlasCondominio CreateAtlasCondominio(
        int codCondom,
        string cnpj,
        string nome)
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
            DtAlteracao = new DateTime(2026, 7, 14, 10, 0, 0),
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
}