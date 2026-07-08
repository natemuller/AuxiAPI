using AuxiAPI.src.Common;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.Services;
using Moq;

namespace AuxiAPI.Tests.ServicesTest;

public class CondominioServiceTest
{
    [Fact]
    public async Task ListarCondominiosAsync_DeveChamarRepositoryListarAsync()
    {
        var repository = new Mock<ICondominioRepository>();
        var condominio = CreateCondominio();
        var query = new VisualizarCondominioQuery();

        repository
            .Setup(x => x.ListarAsync(query, It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio> { condominio }, 1));

        var service = CriarService(repository.Object);
        var resultado = await service.ListarCondominiosAsync(query);

        repository.Verify(x => x.ListarAsync(query, 10), Times.Once);

        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(10, resultado.TamanhoPagina);
        Assert.Equal(1, resultado.TotalItens);
        Assert.Equal(1, resultado.TotalPaginas);

        var dto = Assert.Single(resultado.Itens);
        Assert.Equal(condominio.CodigoDoCondominio, dto.CodigoDoCondominio);
        Assert.Equal(condominio.NomeDoCondominio, dto.NomeDoCondominio);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveMapearCondominioParaDtoCorretamente()
    {
        var repository = new Mock<ICondominioRepository>();
        var condominio = CreateCondominio();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Condominio> { condominio }, 1));

        var service = CriarService(repository.Object);
        var resultado = await service.ListarCondominiosAsync(new VisualizarCondominioQuery());

        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(10, resultado.TamanhoPagina);
        Assert.Equal(1, resultado.TotalItens);
        Assert.Equal(1, resultado.TotalPaginas);

        var dto = Assert.Single(resultado.Itens);

        Assert.Equal(condominio.CodigoDoCondominio, dto.CodigoDoCondominio);
        Assert.Equal(condominio.CNPJDoCondominio, dto.CNPJDoCondominio);
        Assert.Equal(condominio.NomeDoCondominio, dto.NomeDoCondominio);
        Assert.Equal(condominio.Endereco, dto.Endereco);
        Assert.Equal(condominio.NumeroDoEndereco, dto.NumeroDoEndereco);
        Assert.Equal(condominio.EstadoDoEndereco, dto.EstadoDoEndereco);
        Assert.Equal(condominio.CidadeDoEndereco, dto.CidadeDoEndereco);
        Assert.Equal(condominio.BairroDoEndereco, dto.BairroDoEndereco);
        Assert.Equal(condominio.CEPDoEndereco, dto.CEPDoEndereco);
        Assert.Equal(condominio.NumeroDeTorres, dto.NumeroDeTorres);
        Assert.Equal(condominio.NumeroDeUnidades, dto.NumeroDeUnidades);
        Assert.Equal(condominio.Status, dto.Status);
        Assert.Equal(condominio.DataInicial_Administracao, dto.DataInicial_Administracao);
        Assert.Equal(condominio.DataFinal_Administracao, dto.DataFinal_Administracao);
        Assert.Equal(condominio.NomeGerenteDeContas, dto.NomeGerenteDeContas);
        Assert.Equal(condominio.NomeSindico, dto.NomeSindico);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveLancarArgumentException_QuandoCodigoExcederLimite()
    {
        var repository = new Mock<ICondominioRepository>();
        var service = CriarService(repository.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListarCondominiosAsync(new VisualizarCondominioQuery
            {
                CodigoDoCondominio = new string('1', 16)
            }));

        Assert.Equal(MensagensDeErro.CodigoTamanhoExcedido, exception.Message);

        repository.Verify(x => x.ListarAsync(
            It.IsAny<VisualizarCondominioQuery>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveLancarArgumentException_QuandoCnpjExcederLimite()
    {
        var repository = new Mock<ICondominioRepository>();
        var service = CriarService(repository.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListarCondominiosAsync(new VisualizarCondominioQuery
            {
                CNPJDoCondominio = "12.345.678/9012-3456"
            }));

        Assert.Equal(MensagensDeErro.CnpjTamanhoExcedido, exception.Message);

        repository.Verify(x => x.ListarAsync(
            It.IsAny<VisualizarCondominioQuery>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveLancarArgumentException_QuandoNomeExcederLimite()
    {
        var repository = new Mock<ICondominioRepository>();
        var service = CriarService(repository.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListarCondominiosAsync(new VisualizarCondominioQuery
            {
                NomeDoCondominio = new string('n', 201)
            }));

        Assert.Equal(MensagensDeErro.NomeTamanhoExcedido, exception.Message);

        repository.Verify(x => x.ListarAsync(
            It.IsAny<VisualizarCondominioQuery>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveLancarArgumentException_QuandoPaginaForMenorQueUm()
    {
        var repository = new Mock<ICondominioRepository>();
        var service = CriarService(repository.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListarCondominiosAsync(new VisualizarCondominioQuery
            {
                Pagina = 0
            }));

        Assert.Equal(MensagensDeErro.PaginaInvalida, exception.Message);

        repository.Verify(x => x.ListarAsync(
            It.IsAny<VisualizarCondominioQuery>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarDto_QuandoIdExistir()
    {
        var repository = new Mock<ICondominioRepository>();
        var condominio = CreateCondominio();

        repository
            .Setup(x => x.ObterPorIdAsync(1))
            .ReturnsAsync(condominio);

        var service = CriarService(repository.Object);
        var resultado = await service.ObterPorIdAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal(condominio.CodigoDoCondominio, resultado.CodigoDoCondominio);
        Assert.Equal(condominio.NomeDoCondominio, resultado.NomeDoCondominio);

        repository.Verify(x => x.ObterPorIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarKeyNotFoundException_QuandoIdNaoExistir()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ObterPorIdAsync(9999))
            .ReturnsAsync((Condominio?)null);

        var service = CriarService(repository.Object);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ObterPorIdAsync(9999));

        Assert.Contains("9999", exception.Message);

        repository.Verify(x => x.ObterPorIdAsync(9999), Times.Once);
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