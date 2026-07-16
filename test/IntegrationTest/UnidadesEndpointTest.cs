using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

namespace AuxiAPI.Tests.IntegrationTest;

[Collection(PostgresCollectionNames.PostgresCollection)]
public class UnidadesEndpointTest(PostgresTestFixture fixture) : IClassFixture<WebApplicationFactory<Program>>
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
    public async Task GET_api_unidades_DeveRetornar200_ComLista()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasUnidadeDto>>();

        Assert.NotNull(body);
        Assert.NotEmpty(body!.Itens);
        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
        Assert.Equal(3, body.TotalItens);
        Assert.Equal(1, body.TotalPaginas);
    }

    [Fact]
    public async Task GET_api_unidades_DeveFiltrarPorCodCondom()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades?codCondom=5396");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasUnidadeDto>>();

        Assert.NotNull(body);
        Assert.Equal(2, body!.TotalItens);

        Assert.All(body.Itens, unidade =>
        {
            Assert.Equal(5396, unidade.CodCondom);
        });
    }

    [Fact]
    public async Task GET_api_unidades_DeveFiltrarPorNomeCondominoCaseInsensitive()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades?nomeCondomino=joão");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasUnidadeDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);

        Assert.Equal(1, item.IdEconomia);
        Assert.Equal("João Silva", item.NomeCondomino);
    }

    [Fact]
    public async Task GET_api_unidades_DeveFiltrarPorNomeCondominoIgnorandoAcento()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades?nomeCondomino=jose");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasUnidadeDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);

        Assert.Equal(3, item.IdEconomia);
        Assert.Equal("José Almeida", item.NomeCondomino);
    }

    [Fact]
    public async Task GET_api_unidades_DeveFiltrarPorCodCondomENomeCondomino()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades?codCondom=5396&nomeCondomino=joao");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasUnidadeDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);

        Assert.Equal(1, item.IdEconomia);
        Assert.Equal(5396, item.CodCondom);
        Assert.Equal("João Silva", item.NomeCondomino);
    }

    [Fact]
    public async Task GET_api_unidades_idEconomia_DeveRetornar200_QuandoIdEconomiaExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AtlasUnidadeDto>();

        Assert.NotNull(body);
        Assert.Equal(1, body!.IdEconomia);
        Assert.Equal(5396, body.CodCondom);
        Assert.Equal("João Silva", body.NomeCondomino);
    }

    [Fact]
    public async Task GET_api_unidades_idEconomia_DeveRetornar404_QuandoIdEconomiaNaoExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(body.GetProperty("sucesso").GetBoolean());
        Assert.Equal(404, body.GetProperty("status").GetInt32());
        Assert.Contains("não foi encontrada", body.GetProperty("mensagem").GetString());
        Assert.Equal("/api/unidades/999999", body.GetProperty("caminho").GetString());
    }

    [Fact]
    public async Task GET_api_unidades_DeveRetornar400_QuandoPaginaForInvalida()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades?pagina=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(body.GetProperty("sucesso").GetBoolean());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Contains("pagina", body.GetProperty("mensagem").GetString());
    }

    [Fact]
    public async Task GET_api_unidades_DeveRetornar400_QuandoCodCondomForInvalido()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
            return;

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/unidades?codCondom=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(body.GetProperty("sucesso").GetBoolean());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Contains("condomínio", body.GetProperty("mensagem").GetString());
    }

    private async Task SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CondominiosDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync("create extension if not exists unaccent;");

        await RemoverForeignKeysDeAtlasUnidadesAsync(context);

        context.Cache.RemoveRange(context.Cache);
        context.AtlasUnidades.RemoveRange(context.AtlasUnidades);

        await context.SaveChangesAsync();

        context.AtlasUnidades.AddRange(
            CreateAtlasUnidade(
                idEconomia: 1,
                codCondom: 5396,
                codBloco: "A",
                codEconom: "101",
                nomeCondomino: "João Silva"),
            CreateAtlasUnidade(
                idEconomia: 2,
                codCondom: 5396,
                codBloco: "A",
                codEconom: "102",
                nomeCondomino: "Maria Souza"),
            CreateAtlasUnidade(
                idEconomia: 3,
                codCondom: 5400,
                codBloco: "B",
                codEconom: "201",
                nomeCondomino: "José Almeida"));

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