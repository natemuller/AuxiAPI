using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.Tests.RepositoriesTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class UnidadeRepositoryTest(PostgresTestFixture fixture) : IAsyncLifetime
{
    private const int TamanhoPagina = 10;

    private readonly PostgresTestFixture _fixture = fixture;
    private CondominiosDbContext _context = null!;
    private UnidadeRepository _repository = null!;

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

        await _context.Database.ExecuteSqlRawAsync("create extension if not exists unaccent;");

        await RemoverForeignKeysDeAtlasUnidadesAsync();

        _repository = new UnidadeRepository(_context);
    }

    public async Task DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.DisposeAsync();
        }
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarTodasUnidades_QuandoNenhumFiltroForInformado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(
            CriarUnidade(1, 5396, "A", "101", "João Silva"),
            CriarUnidade(2, 5396, "A", "102", "Maria Souza"));

        var resultado = await _repository.ListarAsync(
            new VisualizarUnidadeQuery(),
            TamanhoPagina);

        Assert.NotNull(resultado.Itens);
        Assert.Equal(2, resultado.Itens.Count);
        Assert.Equal(2, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCodCondom()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(
            CriarUnidade(1, 5396, "A", "101", "João Silva"),
            CriarUnidade(2, 5400, "B", "201", "Maria Souza"));

        var resultado = await _repository.ListarAsync(
            new VisualizarUnidadeQuery
            {
                CodCondom = 5396
            },
            TamanhoPagina);

        var unidade = Assert.Single(resultado.Itens);

        Assert.Equal(1, unidade.IdEconomia);
        Assert.Equal(5396, unidade.CodCondom);
        Assert.Equal("João Silva", unidade.NomeCondomino);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorNomeCondomino_IgnorandoMaiusculasMinusculas()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(
            CriarUnidade(1, 5396, "A", "101", "João Silva"),
            CriarUnidade(2, 5396, "A", "102", "Maria Souza"));

        var resultado = await _repository.ListarAsync(
            new VisualizarUnidadeQuery
            {
                NomeCondomino = "joão"
            },
            TamanhoPagina);

        var unidade = Assert.Single(resultado.Itens);

        Assert.Equal(1, unidade.IdEconomia);
        Assert.Equal("João Silva", unidade.NomeCondomino);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorNomeCondomino_IgnorandoAcento()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(
            CriarUnidade(1, 5396, "A", "101", "José Almeida"),
            CriarUnidade(2, 5396, "A", "102", "Maria Souza"));

        var resultado = await _repository.ListarAsync(
            new VisualizarUnidadeQuery
            {
                NomeCondomino = "jose"
            },
            TamanhoPagina);

        var unidade = Assert.Single(resultado.Itens);

        Assert.Equal(1, unidade.IdEconomia);
        Assert.Equal("José Almeida", unidade.NomeCondomino);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCodCondomENomeCondomino()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(
            CriarUnidade(1, 5396, "A", "101", "João Silva"),
            CriarUnidade(2, 5400, "B", "201", "João Silva"),
            CriarUnidade(3, 5396, "A", "102", "Maria Souza"));

        var resultado = await _repository.ListarAsync(
            new VisualizarUnidadeQuery
            {
                CodCondom = 5396,
                NomeCondomino = "joao"
            },
            TamanhoPagina);

        var unidade = Assert.Single(resultado.Itens);

        Assert.Equal(1, unidade.IdEconomia);
        Assert.Equal(5396, unidade.CodCondom);
        Assert.Equal("João Silva", unidade.NomeCondomino);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DevePaginarResultados()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var unidades = Enumerable.Range(1, 12)
            .Select(i => CriarUnidade(
                idEconomia: i,
                codCondom: 5396,
                codBloco: "A",
                codEconom: i.ToString("D3"),
                nomeCondomino: $"Condômino {i:D2}"))
            .ToArray();

        await SeedAsync(unidades);

        var resultado = await _repository.ListarAsync(
            new VisualizarUnidadeQuery
            {
                Pagina = 2
            },
            TamanhoPagina);

        Assert.Equal(2, resultado.Itens.Count);
        Assert.Equal(12, resultado.TotalItens);
        Assert.Equal(11, resultado.Itens[0].IdEconomia);
        Assert.Equal(12, resultado.Itens[1].IdEconomia);
    }

    [Fact]
    public async Task ObterPorIdEconomiaAsync_DeveRetornarUnidade_QuandoIdEconomiaExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CriarUnidade(123, 5396, "A", "101", "João Silva"));

        var resultado = await _repository.ObterPorIdEconomiaAsync(123);

        Assert.NotNull(resultado);
        Assert.Equal(123, resultado!.IdEconomia);
        Assert.Equal(5396, resultado.CodCondom);
        Assert.Equal("João Silva", resultado.NomeCondomino);
    }

    [Fact]
    public async Task ObterPorIdEconomiaAsync_DeveRetornarNull_QuandoIdEconomiaNaoExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var resultado = await _repository.ObterPorIdEconomiaAsync(999999);

        Assert.Null(resultado);
    }

    private async Task SeedAsync(params AtlasUnidade[] unidades)
    {
        _context.AtlasUnidades.AddRange(unidades);
        await _context.SaveChangesAsync();
    }

    private static AtlasUnidade CriarUnidade(
        int idEconomia,
        int codCondom,
        string codBloco,
        string codEconom,
        string nomeCondomino)
    {
        return new AtlasUnidade
        {
            IdEconomia = idEconomia,
            CodCondom = codCondom,
            CodBloco = codBloco,
            CodEconom = codEconom,
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
}