using System.Net;
using System.Net.Http.Json;
using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuxiAPI.Tests.IntegrationTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class CondominiosAuthorizationEndpointTest(PostgresTestFixture fixture, ITestOutputHelper output)
{
    private readonly PostgresTestFixture _fixture = fixture;

    [Fact]
    public async Task GET_api_condominios_DeveRetornar401_QuandoUsuarioNaoEstiverAutenticado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        using var factory = CriarFactoryNaoAutenticada();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/condominios");

        output.WriteLine($"Status sem autenticação: {(int)response.StatusCode} {response.StatusCode}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GET_api_condominios_DeveRetornar200_QuandoUsuarioEstiverAutenticado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        using var factory = CriarFactoryAutenticada();

        await SeedAsync(factory);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/condominios");

        output.WriteLine($"Status com autenticação fake: {(int)response.StatusCode} {response.StatusCode}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<InformacoesCondominioDto>>();

        Assert.NotNull(body);
        Assert.NotEmpty(body!.Itens);
        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
        Assert.Equal(1, body.TotalItens);

        var item = Assert.Single(body.Itens);
        Assert.Equal("0001", item.CodigoDoCondominio);
        Assert.Equal("Residencial Brasil-Hexa", item.NomeDoCondominio);
    }

    private WebApplicationFactory<Program> CriarFactoryNaoAutenticada()
    {
        return CriarFactoryBase()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = TestUnauthHandler.SchemeName;
                            options.DefaultChallengeScheme = TestUnauthHandler.SchemeName;
                        })
                        .AddScheme<AuthenticationSchemeOptions, TestUnauthHandler>(
                            TestUnauthHandler.SchemeName,
                            _ => { });
                });
            });
    }

    private WebApplicationFactory<Program> CriarFactoryAutenticada()
    {
        return CriarFactoryBase()
            .WithWebHostBuilder(builder =>
            {
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
    }

    private WebApplicationFactory<Program> CriarFactoryBase()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:SupabaseConnection"] = _fixture.ConnectionString
                    });
                });
            });
    }

    private static async Task SeedAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CondominiosDbContext>();

        await context.Database.EnsureCreatedAsync();

        context.Cache.RemoveRange(context.Cache);
        context.Condominios.RemoveRange(context.Condominios);
        await context.SaveChangesAsync();

        context.Condominios.Add(CreateCondominio("0001", "12345678000101", "Residencial Brasil-Hexa"));
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