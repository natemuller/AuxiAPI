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

        await SeedAsync(CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"),
            CreateCondominio("0002", "12543867000101", "Residencial Vivendas do Pelé"));

        var resultado = await _repository.ListarAsync(new VisualizarCondominioQuery());

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCodigoDoCondominio_ComCodigoSemZeros()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"));

        var resultado = await _repository.ListarAsync(new VisualizarCondominioQuery { CodigoDoCondominio = "1" });

        var condominio = Assert.Single(resultado);
        Assert.Equal("0001", condominio.CodigoDoCondominio);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCodigoDoCondominio_ComCodigoCompleto()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"));

        var resultado = await _repository.ListarAsync(new VisualizarCondominioQuery { CodigoDoCondominio = "0001" });

        var condominio = Assert.Single(resultado);
        Assert.Equal("0001", condominio.CodigoDoCondominio);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCnpjSemMascara()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"));

        var resultado = await _repository.ListarAsync(new VisualizarCondominioQuery { CNPJDoCondominio = "12345678000101" });

        var condominio = Assert.Single(resultado);
        Assert.Equal("12345678000101", condominio.CNPJDoCondominio);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorCnpjComMascara()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"));

        var resultado = await _repository.ListarAsync(new VisualizarCondominioQuery { CNPJDoCondominio = "12.345.678/0001-01" });

        var condominio = Assert.Single(resultado);
        Assert.Equal("12345678000101", condominio.CNPJDoCondominio);
    }

    [Theory]
    [InlineData("brasil")]
    [InlineData("BRASIL")]
    public async Task ListarAsync_DeveFiltrarPorNomeIgnorandoMaiusculasMinusculas(string nome)
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"));

        var resultado = await _repository.ListarAsync(new VisualizarCondominioQuery { NomeDoCondominio = nome });

        var condominio = Assert.Single(resultado);
        Assert.Equal("Residencial Brasil-Hexa", condominio.NomeDoCondominio);
    }

    [Fact]
    public async Task ListarAsync_DeveCombinarFiltros_QuandoMaisDeUmFiltroForInformado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync(
            CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"),
            CreateCondominio("0002", "12543867000101", "Residencial Vivendas do Pelé"));

        var resultado = await _repository.ListarAsync(new VisualizarCondominioQuery
        {
            CodigoDoCondominio = "0001",
            NomeDoCondominio = "Brasil"
        });

        var condominio = Assert.Single(resultado);
        Assert.Equal("0001", condominio.CodigoDoCondominio);
        Assert.Equal("Residencial Brasil-Hexa", condominio.NomeDoCondominio);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarListaVazia_QuandoNenhumRegistroForEncontrado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var resultado = await _repository.ListarAsync(new VisualizarCondominioQuery { CodigoDoCondominio = "9999" });

        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarCondominio_QuandoIdExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var condominio = CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa");
        await SeedAsync(condominio);

        var resultado = await _repository.ObterPorIdAsync(condominio.Id);

        Assert.NotNull(resultado);
        Assert.Equal(condominio.Id, resultado!.Id);
        Assert.Equal("0001", resultado.CodigoDoCondominio);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoIdNaoExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var resultado = await _repository.ObterPorIdAsync(9999);

        Assert.Null(resultado);
    }

    private async Task SeedAsync(params Condominio[] condominios)
    {
        _context.Condominios.AddRange(condominios);
        await _context.SaveChangesAsync();
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
