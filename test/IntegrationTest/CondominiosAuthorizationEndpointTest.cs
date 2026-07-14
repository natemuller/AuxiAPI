using System.Net;
using System.Net.Http.Json;
using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasCondominioDto>>();

        Assert.NotNull(body);
        Assert.NotEmpty(body!.Itens);
        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
        Assert.Equal(1, body.TotalItens);

        var item = Assert.Single(body.Itens);

        Assert.Equal(5396, item.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", item.NomeCondom);
        Assert.Equal("17474690000113", item.Cnpj);
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

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync("create extension if not exists unaccent;");

        context.Cache.RemoveRange(context.Cache);
        context.AtlasCondominios.RemoveRange(context.AtlasCondominios);

        await context.SaveChangesAsync();

        context.AtlasCondominios.Add(CreateAtlasCondominio(
            codCondom: 5396,
            cnpj: "17474690000113",
            nome: "SOLAR DI TOSCANA"));

        await context.SaveChangesAsync();
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