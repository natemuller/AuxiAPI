using AuxiAPI.src.Controllers;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuxiAPI.Tests.ControllersTest;

public class CondominiosControllerTest
{
    [Fact]
    public async Task GetAll_DeveRetornarOk_ComListaPaginadaDeCondominiosAtlas()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<AtlasCondominio> { CreateAtlasCondominio() }, 1));

        var controller = new CondominiosController(CriarService(repository.Object));

        var resultado = await controller.GetAll(new VisualizarCondominioQuery());

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(200, okResult.StatusCode);

        var body = Assert.IsType<ResultadoPaginadoDto<AtlasCondominioDto>>(okResult.Value);

        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
        Assert.Equal(1, body.TotalItens);
        Assert.Equal(1, body.TotalPaginas);

        var item = Assert.Single(body.Itens);

        Assert.Equal(5396, item.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", item.NomeCondom);
        Assert.Equal("17474690000113", item.Cnpj);
    }

    [Fact]
    public async Task GetByCodCondom_DeveRetornarOk_QuandoCodCondomExistir()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ObterPorCodCondomAsync(5396))
            .ReturnsAsync(CreateAtlasCondominio());

        var controller = new CondominiosController(CriarService(repository.Object));

        var resultado = await controller.GetByCodCondom(5396);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var body = Assert.IsType<AtlasCondominioDto>(okResult.Value);

        Assert.Equal(5396, body.CodCondom);
        Assert.Equal("SOLAR DI TOSCANA", body.NomeCondom);
        Assert.Equal("17474690000113", body.Cnpj);
    }

    private static CondominioService CriarService(ICondominioRepository repository)
    {
        var cacheService = new Mock<IDatabaseCacheService>();

        cacheService
            .Setup(x => x.ObterAsync<AtlasCondominioDto>(It.IsAny<string>()))
            .ReturnsAsync((AtlasCondominioDto?)null);

        cacheService
            .Setup(x => x.ObterAsync<ResultadoPaginadoDto<AtlasCondominioDto>>(It.IsAny<string>()))
            .ReturnsAsync((ResultadoPaginadoDto<AtlasCondominioDto>?)null);

        cacheService
            .Setup(x => x.SalvarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<AtlasCondominioDto>(),
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        cacheService
            .Setup(x => x.SalvarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<ResultadoPaginadoDto<AtlasCondominioDto>>(),
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        return new CondominioService(repository, cacheService.Object);
    }

    private static AtlasCondominio CreateAtlasCondominio()
    {
        return new AtlasCondominio
        {
            CodCondom = 5396,
            NomeCondom = "SOLAR DI TOSCANA",
            Ativo = "S",
            Cnpj = "17474690000113",
            Cei = null,
            InscrMunicip = null,
            QtdBlocos = 1,
            QtdUnidades = 9,
            TotalFracao = 10000000000,
            DiaVencDoc = 10,
            DataInicioAdm = 43399,
            DataDistrato = null,
            MotivoDistrato = null,
            Assessor = "GERENTE TESTE",
            Filial = "PORTO ALEGRE",
            Agencia = "AGENCIA TESTE",
            Sindico = "SINDICO TESTE",
            SubSindico = null,
            Conselheiro = null,
            Gestor = null,
            ConselhoFiscal = null,
            ConselhoConsultivo = null,
            ConselhoSuplente = null,
            TipoCondominio = "Residencial",
            TipoCategoria = "Condomínio",
            DtAlteracao = DateTime.UtcNow,
            TipoLograd = "RUA",
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