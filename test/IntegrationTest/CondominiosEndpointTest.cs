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
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasCondominioDto>>();

        Assert.NotNull(body);
        Assert.NotEmpty(body!.Itens);
        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
        Assert.Equal(2, body.TotalItens);

        var primeiro = body.Itens.First();

        Assert.Equal(5396, primeiro.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", primeiro.NomeCondom);
        Assert.Equal("17474690000113", primeiro.Cnpj);
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

        var response = await client.GetAsync("/api/condominios?cnpj=17.474.690/0001-13");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasCondominioDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);

        Assert.Equal(5396, item.CodCondom);
        Assert.Equal("17474690000113", item.Cnpj);
        Assert.Equal("SOLAR DI TOSCANA", item.NomeCondom);
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

        var response = await client.GetAsync("/api/condominios?nomeCondom=solar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasCondominioDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);

        Assert.Equal(5396, item.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", item.NomeCondom);
    }

    [Fact]
    public async Task GET_api_condominios_DeveFiltrarPorNomeIgnorandoAcento()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/condominios?nomeCondom=pele");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasCondominioDto>>();

        Assert.NotNull(body);

        var item = Assert.Single(body!.Itens);

        Assert.Equal(5400, item.CodCondom);
        Assert.Equal("Residencial Vivendas do Pelé", item.NomeCondom);
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

        var response = await client.GetAsync("/api/condominios?nomeCondom=inexistente");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ResultadoPaginadoDto<AtlasCondominioDto>>();

        Assert.NotNull(body);
        Assert.Empty(body!.Itens);
        Assert.Equal(0, body.TotalItens);
    }

    [Fact]
    public async Task GET_api_condominios_codcondom_DeveRetornar200_QuandoCodCondomExistir()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        await SeedAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/condominios/5396");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AtlasCondominioDto>();

        Assert.NotNull(body);
        Assert.Equal(5396, body!.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", body.NomeCondom);
        Assert.Equal("17474690000113", body.Cnpj);
        Assert.Equal("Porto Alegre", body.Cidade);
        Assert.Equal("RS", body.Uf);
    }

    [Fact]
    public async Task GET_api_condominios_codcondom_DeveRetornar404_QuandoCodCondomNaoExistir()
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
    public async Task GET_api_condominios_DeveRetornar400_QuandoCnpjExcederLimite()
    {
        if (!string.IsNullOrWhiteSpace(_fixture.SkipReason))
        {
            return;
        }

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/condominios?cnpj=1234567890123456");

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

        var response = await client.GetAsync($"/api/condominios?nomeCondom={nome}");

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

        await context.Database.ExecuteSqlRawAsync("create extension if not exists unaccent;");

        context.Cache.RemoveRange(context.Cache);
        context.AtlasUnidades.RemoveRange(context.AtlasUnidades);
        context.AtlasBlocos.RemoveRange(context.AtlasBlocos);

        await context.SaveChangesAsync();

        context.AtlasCondominios.RemoveRange(context.AtlasCondominios);

        await context.SaveChangesAsync();

        context.AtlasCondominios.AddRange(
            CreateAtlasCondominio(
                codCondom: 5396,
                cnpj: "17474690000113",
                nome: "SOLAR DI TOSCANA"),
            CreateAtlasCondominio(
                codCondom: 5400,
                cnpj: "12543867000101",
                nome: "Residencial Vivendas do Pelé"));

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
