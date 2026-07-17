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
public class UnidadesAuthorizationEndpointTest(
    PostgresTestFixture fixture,
    ITestOutputHelper output)
{
    private readonly PostgresTestFixture _fixture = fixture;

    [Fact]
    public async Task GET_api_unidades_DeveRetornar401_QuandoUsuarioNaoEstiverAutenticado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        using var factory = CriarFactoryNaoAutenticada();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/unidades");

        output.WriteLine($"Status sem autenticação: {(int)response.StatusCode} {response.StatusCode}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GET_api_unidades_DeveRetornar200_QuandoUsuarioEstiverAutenticado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        using var factory = CriarFactoryAutenticada();

        await SeedAsync(factory);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/unidades");

        output.WriteLine($"Status com autenticação fake: {(int)response.StatusCode} {response.StatusCode}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasUnidadeDto>>();

        Assert.NotNull(body);
        Assert.NotEmpty(body!.Itens);
        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
        Assert.Equal(1, body.TotalItens);

        var item = Assert.Single(body.Itens);

        Assert.Equal(1, item.IdEconomia);
        Assert.Equal(5396, item.CodCondom);
        Assert.Equal("João Silva", item.NomeCondomino);
    }

    [Fact]
    public async Task GET_api_unidades_idEconomia_DeveRetornar401_QuandoUsuarioNaoEstiverAutenticado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        using var factory = CriarFactoryNaoAutenticada();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/unidades/1");

        output.WriteLine($"Status sem autenticação por idEconomia: {(int)response.StatusCode} {response.StatusCode}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GET_api_unidades_idEconomia_DeveRetornar200_QuandoUsuarioEstiverAutenticado()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        using var factory = CriarFactoryAutenticada();

        await SeedAsync(factory);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/unidades/1");

        output.WriteLine($"Status com autenticação fake por idEconomia: {(int)response.StatusCode} {response.StatusCode}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AtlasUnidadeDto>();

        Assert.NotNull(body);
        Assert.Equal(1, body!.IdEconomia);
        Assert.Equal(5396, body.CodCondom);
        Assert.Equal("João Silva", body.NomeCondomino);
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

        await context.Database.ExecuteSqlRawAsync("create extension if not exists unaccent;");

        await RemoverForeignKeysDeAtlasUnidadesAsync(context);

        context.Cache.RemoveRange(context.Cache);
        context.AtlasUnidades.RemoveRange(context.AtlasUnidades);

        await context.SaveChangesAsync();

        context.AtlasUnidades.Add(CreateAtlasUnidade(
            idEconomia: 1,
            codCondom: 5396,
            codBloco: "A",
            codEconom: "101",
            nomeCondomino: "João Silva"));

        await context.SaveChangesAsync();
    }

    private static AtlasUnidade CreateAtlasUnidade(
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

    private static async Task RemoverForeignKeysDeAtlasUnidadesAsync(
        CondominiosDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
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
