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
        var condominio = CreateAtlasCondominio();
        var query = new VisualizarCondominioQuery();

        repository
            .Setup(x => x.ListarAsync(query, It.IsAny<int>()))
            .ReturnsAsync((new List<AtlasCondominio> { condominio }, 1));

        var service = CriarService(repository.Object);

        var resultado = await service.ListarCondominiosAsync(query);

        repository.Verify(x => x.ListarAsync(query, 10), Times.Once);

        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(10, resultado.TamanhoPagina);
        Assert.Equal(1, resultado.TotalItens);
        Assert.Equal(1, resultado.TotalPaginas);

        var dto = Assert.Single(resultado.Itens);

        Assert.Equal(condominio.CodCondom, dto.CodCondom);
        Assert.Equal(condominio.NomeCondom, dto.NomeCondom);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveMapearAtlasCondominioParaDtoCorretamente()
    {
        var repository = new Mock<ICondominioRepository>();
        var condominio = CreateAtlasCondominio();

        repository
            .Setup(x => x.ListarAsync(
                It.IsAny<VisualizarCondominioQuery>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<AtlasCondominio> { condominio }, 1));

        var service = CriarService(repository.Object);

        var resultado = await service.ListarCondominiosAsync(new VisualizarCondominioQuery());

        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(10, resultado.TamanhoPagina);
        Assert.Equal(1, resultado.TotalItens);
        Assert.Equal(1, resultado.TotalPaginas);

        var dto = Assert.Single(resultado.Itens);

        Assert.Equal(condominio.CodCondom, dto.CodCondom);
        Assert.Equal(condominio.NomeCondom, dto.NomeCondom);
        Assert.Equal(condominio.Ativo, dto.Ativo);
        Assert.Equal(condominio.Cnpj, dto.Cnpj);
        Assert.Equal(condominio.Cei, dto.Cei);
        Assert.Equal(condominio.InscrMunicip, dto.InscrMunicip);
        Assert.Equal(condominio.QtdBlocos, dto.QtdBlocos);
        Assert.Equal(condominio.QtdUnidades, dto.QtdUnidades);
        Assert.Equal(condominio.TotalFracao, dto.TotalFracao);
        Assert.Equal(condominio.DiaVencDoc, dto.DiaVencDoc);
        Assert.Equal(condominio.DataInicioAdm, dto.DataInicioAdm);
        Assert.Equal(condominio.DataDistrato, dto.DataDistrato);
        Assert.Equal(condominio.MotivoDistrato, dto.MotivoDistrato);
        Assert.Equal(condominio.Assessor, dto.Assessor);
        Assert.Equal(condominio.Filial, dto.Filial);
        Assert.Equal(condominio.Agencia, dto.Agencia);
        Assert.Equal(condominio.Sindico, dto.Sindico);
        Assert.Equal(condominio.SubSindico, dto.SubSindico);
        Assert.Equal(condominio.Conselheiro, dto.Conselheiro);
        Assert.Equal(condominio.Gestor, dto.Gestor);
        Assert.Equal(condominio.ConselhoFiscal, dto.ConselhoFiscal);
        Assert.Equal(condominio.ConselhoConsultivo, dto.ConselhoConsultivo);
        Assert.Equal(condominio.ConselhoSuplente, dto.ConselhoSuplente);
        Assert.Equal(condominio.TipoCondominio, dto.TipoCondominio);
        Assert.Equal(condominio.TipoCategoria, dto.TipoCategoria);
        Assert.Equal(condominio.DtAlteracao, dto.DtAlteracao);
        Assert.Equal(condominio.TipoLograd, dto.TipoLograd);
        Assert.Equal(condominio.Lograd, dto.Lograd);
        Assert.Equal(condominio.Numero, dto.Numero);
        Assert.Equal(condominio.Bairro, dto.Bairro);
        Assert.Equal(condominio.Cidade, dto.Cidade);
        Assert.Equal(condominio.Cep8Log, dto.Cep8Log);
        Assert.Equal(condominio.Uf, dto.Uf);
        Assert.Equal(condominio.CodPessoaSindico, dto.CodPessoaSindico);
        Assert.Equal(condominio.NomeSindico, dto.NomeSindico);
        Assert.Equal(condominio.CpfDocnpj, dto.CpfDocnpj);
        Assert.Equal(condominio.CondGarantido, dto.CondGarantido);
        Assert.Equal(condominio.TipoConta, dto.TipoConta);
        Assert.Equal(condominio.ObsCobranca, dto.ObsCobranca);
        Assert.Equal(condominio.Garantidora, dto.Garantidora);
    }

    [Fact]
    public async Task ListarCondominiosAsync_DeveLancarArgumentException_QuandoCnpjExcederLimite()
    {
        var repository = new Mock<ICondominioRepository>();
        var service = CriarService(repository.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListarCondominiosAsync(new VisualizarCondominioQuery
            {
                Cnpj = "12.345.678/9012-3456"
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
                NomeCondom = new string('n', 201)
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
    public async Task ObterPorCodCondomAsync_DeveRetornarDto_QuandoCodCondomExistir()
    {
        var repository = new Mock<ICondominioRepository>();
        var condominio = CreateAtlasCondominio();

        repository
            .Setup(x => x.ObterPorCodCondomAsync(5396))
            .ReturnsAsync(condominio);

        var service = CriarService(repository.Object);

        var resultado = await service.ObterPorCodCondomAsync(5396);

        Assert.NotNull(resultado);
        Assert.Equal(condominio.CodCondom, resultado.CodCondom);
        Assert.Equal(condominio.NomeCondom, resultado.NomeCondom);
        Assert.Equal(condominio.Cnpj, resultado.Cnpj);

        repository.Verify(x => x.ObterPorCodCondomAsync(5396), Times.Once);
    }

    [Fact]
    public async Task ObterPorCodCondomAsync_DeveLancarKeyNotFoundException_QuandoCodCondomNaoExistir()
    {
        var repository = new Mock<ICondominioRepository>();

        repository
            .Setup(x => x.ObterPorCodCondomAsync(9999))
            .ReturnsAsync((AtlasCondominio?)null);

        var service = CriarService(repository.Object);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ObterPorCodCondomAsync(9999));

        Assert.Contains("9999", exception.Message);

        repository.Verify(x => x.ObterPorCodCondomAsync(9999), Times.Once);
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