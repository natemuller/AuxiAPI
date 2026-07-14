using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.Tests.RepositoriesTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class CondominioRepositoryTest(PostgresTestFixture fixture) : IAsyncLifetime
{
    private const int TamanhoPagina = 10;

    private readonly PostgresTestFixture _fixture = fixture;
    private CondominiosDbContext _context = null!;
    private CondominioRepository _repository = null!;

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

        _repository = new CondominioRepository(_context);
    }

    public async Task DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.DisposeAsync();
        }
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarTodos_QuandoNenhumFiltroForInformado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(
            CreateAtlasCondominio(5396, "17474690000113", "SOLAR DI TOSCANA"),
            CreateAtlasCondominio(5400, "12543867000101", "Residencial Vivendas do Pelé"));

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery(),
            TamanhoPagina);

        Assert.NotNull(resultado.Itens);
        Assert.Equal(2, resultado.Itens.Count);
        Assert.Equal(2, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCnpjSemMascara()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateAtlasCondominio(
            codCondom: 5396,
            cnpj: "17474690000113",
            nome: "SOLAR DI TOSCANA"));

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                Cnpj = "17474690000113"
            },
            TamanhoPagina);

        var condominio = Assert.Single(resultado.Itens);

        Assert.Equal(5396, condominio.CodCondom);
        Assert.Equal("17474690000113", condominio.Cnpj);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCnpjComMascara_QuandoBancoEstiverSemMascara()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateAtlasCondominio(
            codCondom: 5396,
            cnpj: "17474690000113",
            nome: "SOLAR DI TOSCANA"));

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                Cnpj = "17.474.690/0001-13"
            },
            TamanhoPagina);

        var condominio = Assert.Single(resultado.Itens);

        Assert.Equal(5396, condominio.CodCondom);
        Assert.Equal("17474690000113", condominio.Cnpj);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCnpjSemMascara_QuandoBancoEstiverComMascara()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateAtlasCondominio(
            codCondom: 5396,
            cnpj: "17.474.690/0001-13",
            nome: "SOLAR DI TOSCANA"));

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                Cnpj = "17474690000113"
            },
            TamanhoPagina);

        var condominio = Assert.Single(resultado.Itens);

        Assert.Equal(5396, condominio.CodCondom);
        Assert.Equal("17.474.690/0001-13", condominio.Cnpj);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Theory]
    [InlineData("solar")]
    [InlineData("SOLAR")]
    [InlineData("Solar")]
    public async Task ListarAsync_DeveFiltrarPorNomeIgnorandoMaiusculasMinusculas(string nome)
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateAtlasCondominio(
            codCondom: 5396,
            cnpj: "17474690000113",
            nome: "SOLAR DI TOSCANA"));

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                NomeCondom = nome
            },
            TamanhoPagina);

        var condominio = Assert.Single(resultado.Itens);

        Assert.Equal(5396, condominio.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", condominio.NomeCondom);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Theory]
    [InlineData("pele")]
    [InlineData("PELÉ")]
    [InlineData("Pelé")]
    public async Task ListarAsync_DeveFiltrarPorNomeIgnorandoAcentos(string nome)
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateAtlasCondominio(
            codCondom: 5400,
            cnpj: "12543867000101",
            nome: "Residencial Vivendas do Pelé"));

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                NomeCondom = nome
            },
            TamanhoPagina);

        var condominio = Assert.Single(resultado.Itens);

        Assert.Equal(5400, condominio.CodCondom);
        Assert.Equal("Residencial Vivendas do Pelé", condominio.NomeCondom);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveCombinarFiltros_QuandoCnpjENomeForemInformados()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(
            CreateAtlasCondominio(5396, "17474690000113", "SOLAR DI TOSCANA"),
            CreateAtlasCondominio(5400, "12543867000101", "Residencial Vivendas do Pelé"));

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                Cnpj = "17474690000113",
                NomeCondom = "solar"
            },
            TamanhoPagina);

        var condominio = Assert.Single(resultado.Itens);

        Assert.Equal(5396, condominio.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", condominio.NomeCondom);
        Assert.Equal("17474690000113", condominio.Cnpj);
        Assert.Equal(1, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarListaVazia_QuandoNenhumRegistroForEncontrado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                NomeCondom = "condominio inexistente"
            },
            TamanhoPagina);

        Assert.NotNull(resultado.Itens);
        Assert.Empty(resultado.Itens);
        Assert.Equal(0, resultado.TotalItens);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarPrimeiraPagina_ComDezItens()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var condominios = Enumerable.Range(1, 25)
            .Select(CreateAtlasCondominioComIndice)
            .ToArray();

        await SeedAsync(condominios);

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                Pagina = 1
            },
            TamanhoPagina);

        Assert.Equal(10, resultado.Itens.Count);
        Assert.Equal(25, resultado.TotalItens);
        Assert.Equal(1, resultado.Itens.First().CodCondom);
        Assert.Equal(10, resultado.Itens.Last().CodCondom);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarSegundaPagina_ComProximosDezItens()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var condominios = Enumerable.Range(1, 25)
            .Select(CreateAtlasCondominioComIndice)
            .ToArray();

        await SeedAsync(condominios);

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                Pagina = 2
            },
            TamanhoPagina);

        Assert.Equal(10, resultado.Itens.Count);
        Assert.Equal(25, resultado.TotalItens);
        Assert.Equal(11, resultado.Itens.First().CodCondom);
        Assert.Equal(20, resultado.Itens.Last().CodCondom);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarUltimaPagina_ComItensRestantes()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var condominios = Enumerable.Range(1, 25)
            .Select(CreateAtlasCondominioComIndice)
            .ToArray();

        await SeedAsync(condominios);

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                Pagina = 3
            },
            TamanhoPagina);

        Assert.Equal(5, resultado.Itens.Count);
        Assert.Equal(25, resultado.TotalItens);
        Assert.Equal(21, resultado.Itens.First().CodCondom);
        Assert.Equal(25, resultado.Itens.Last().CodCondom);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarListaVazia_QuandoPaginaNaoTiverItens()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var condominios = Enumerable.Range(1, 50)
            .Select(CreateAtlasCondominioComIndice)
            .ToArray();

        await SeedAsync(condominios);

        var resultado = await _repository.ListarAsync(
            new VisualizarCondominioQuery
            {
                Pagina = 6
            },
            TamanhoPagina);

        Assert.Empty(resultado.Itens);
        Assert.Equal(50, resultado.TotalItens);
    }

    [Fact]
    public async Task ObterPorCodCondomAsync_DeveRetornarCondominio_QuandoCodCondomExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var condominio = CreateAtlasCondominio(
            codCondom: 5396,
            cnpj: "17474690000113",
            nome: "SOLAR DI TOSCANA");

        await SeedAsync(condominio);

        var resultado = await _repository.ObterPorCodCondomAsync(5396);

        Assert.NotNull(resultado);
        Assert.Equal(5396, resultado!.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", resultado.NomeCondom);
        Assert.Equal("17474690000113", resultado.Cnpj);
    }

    [Fact]
    public async Task ObterPorCodCondomAsync_DeveRetornarNull_QuandoCodCondomNaoExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var resultado = await _repository.ObterPorCodCondomAsync(9999);

        Assert.Null(resultado);
    }

    private async Task SeedAsync(params AtlasCondominio[] condominios)
    {
        _context.AtlasCondominios.AddRange(condominios);
        await _context.SaveChangesAsync();
    }

    private static AtlasCondominio CreateAtlasCondominioComIndice(int indice)
    {
        return CreateAtlasCondominio(
            codCondom: indice,
            cnpj: indice.ToString().PadLeft(14, '0'),
            nome: $"Residencial Teste {indice:D2}");
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
}