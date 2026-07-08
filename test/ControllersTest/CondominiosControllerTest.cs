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
    public async Task GetAll_DeveRetornarOk_ComListaPaginadaDeCondominios()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio> { CreateCondominio() }, 1));

        var controller = new CondominiosController(CriarService(repository.Object));

        var resultado = await controller.GetAll(new VisualizarCondominioQuery());

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(200, okResult.StatusCode);

        var body = Assert.IsType<ResultadoPaginadoDto<InformacoesCondominioDto>>(okResult.Value);

        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
        Assert.Equal(1, body.TotalItens);
        Assert.Equal(1, body.TotalPaginas);
        Assert.Single(body.Itens);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoIdExistir()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ObterPorIdAsync(1))
            .ReturnsAsync(CreateCondominio());

        var controller = new CondominiosController(CriarService(repository.Object));

        var resultado = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var body = Assert.IsType<InformacoesCondominioDto>(okResult.Value);

        Assert.Equal("0001", body.CodigoDoCondominio);
    }

    private static CondominioService CriarService(ICondominioRepository repository)
    {
        var cacheService = new Mock<IDatabaseCacheService>();

        cacheService
            .Setup(x => x.ObterAsync<InformacoesCondominioDto>(It.IsAny<string>()))
            .ReturnsAsync((InformacoesCondominioDto?)null);

        cacheService
            .Setup(x => x.ObterAsync<ResultadoPaginadoDto<InformacoesCondominioDto>>(It.IsAny<string>()))
            .ReturnsAsync((ResultadoPaginadoDto<InformacoesCondominioDto>?)null);

        cacheService
            .Setup(x => x.SalvarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<InformacoesCondominioDto>(),
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        cacheService
            .Setup(x => x.SalvarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<ResultadoPaginadoDto<InformacoesCondominioDto>>(),
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        return new CondominioService(repository, cacheService.Object);
    }

    private static Condominio CreateCondominio()
    {
        return new Condominio
        {
            CodigoDoCondominio = "0001",
            CNPJDoCondominio = "12345678000101",
            NomeDoCondominio = "Residencial Brasil-Hexa",
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