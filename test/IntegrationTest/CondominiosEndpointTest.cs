using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;

namespace AuxiAPI.Tests.IntegrationTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class CondominiosEndpointTest(PostgresTestFixture fixture) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly PostgresTestFixture _fixture = fixture;
    
    private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SupabaseConnection"] = fixture.ConnectionString
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        _ => { });
            });
        });

    [Fact]
    public async Task GET_api_condominios_DeveRetornar200_ComLista()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<InformacoesCondominioDto>>();

        Assert.NotNull(body);
        Assert.NotEmpty(body!.Itens);
        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
    }

    [Fact]
    public async Task GET_api_condominios_DeveFiltrarPorCodigo()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios?codigoDoCondominio=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<InformacoesCondominioDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);
        Assert.Equal("0001", item.CodigoDoCondominio);
    }

    [Fact]
    public async Task GET_api_condominios_DeveFiltrarPorCnpjComMascara()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios?cnpjDoCondominio=12.345.678/0001-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<InformacoesCondominioDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);
        Assert.Equal("12345678000101", item.CNPJDoCondominio);
    }

    [Fact]
    public async Task GET_api_condominios_DeveFiltrarPorNomeCaseInsensitive()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios?nomeDoCondominio=brasil");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<InformacoesCondominioDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);
        Assert.Equal("Residencial Brasil-Hexa", item.NomeDoCondominio);
    }

    [Fact]
    public async Task GET_api_condominios_DeveRetornarListaVazia_QuandoFiltroNaoEncontrarNada()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios?codigoDoCondominio=9999");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<InformacoesCondominioDto>>();

        Assert.NotNull(body);
        Assert.Empty(body!.Itens);
        Assert.Equal(0, body.TotalItens);
    }

    [Fact]
    public async Task GET_api_condominios_id_DeveRetornar200_QuandoIdExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InformacoesCondominioDto>();

        Assert.NotNull(body);
        Assert.Equal("0001", body!.CodigoDoCondominio);
    }

    [Fact]
    public async Task GET_api_condominios_id_DeveRetornar404_QuandoIdNaoExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(body.GetProperty("sucesso").GetBoolean());
        Assert.Equal(404, body.GetProperty("status").GetInt32());
        Assert.Contains("não foi encontrado", body.GetProperty("mensagem").GetString());
        Assert.Equal("/api/condominios/9999", body.GetProperty("caminho").GetString());
    }

    [Fact]
    public async Task GET_api_condominios_DeveRetornar400_QuandoCodigoExcederLimite()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios?codigoDoCondominio=1234567890123456");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(body.GetProperty("sucesso").GetBoolean());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Contains("codigo de busca", body.GetProperty("mensagem").GetString());
    }

    [Fact]
    public async Task GET_api_condominios_DeveRetornar400_QuandoCnpjExcederLimite()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/condominios?cnpjDoCondominio=1234567890123456");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(body.GetProperty("sucesso").GetBoolean());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Contains("cnpj de busca", body.GetProperty("mensagem").GetString());
    }

    [Fact]
    public async Task GET_api_condominios_DeveRetornar400_QuandoNomeExcederLimite()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        var nome = new string('n', 201);

        using var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/condominios?nomeDoCondominio={nome}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(body.GetProperty("sucesso").GetBoolean());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Contains("nome de busca", body.GetProperty("mensagem").GetString());
    }

    private async Task SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CondominiosDbContext>();

        await context.Database.EnsureCreatedAsync();

        context.Cache.RemoveRange(context.Cache);
        context.Condominios.RemoveRange(context.Condominios);
        await context.SaveChangesAsync();

        context.Condominios.AddRange(
            CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"));

        await context.SaveChangesAsync();
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