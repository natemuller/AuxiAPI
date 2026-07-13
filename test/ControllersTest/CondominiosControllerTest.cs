using AuxiAPI.src.DTOs;

namespace AuxiAPI.Tests.DTOsTest;

public class VisualizarCondominioQueryTest
{
    [Fact]
    public void CnpjDeveNormalizarCnpjComMascara()
    {
        var query = new VisualizarCondominioQuery
        {
            Cnpj = "12.345.678/0001-01"
        };

        Assert.Equal("12345678000101", query.Cnpj);
    }

    [Fact]
    public void CnpjDeveManterNullQuandoValorForNull()
    {
        var query = new VisualizarCondominioQuery
        {
            Cnpj = null
        };

        Assert.Null(query.Cnpj);
    }

    [Fact]
    public void CnpjDeveRemoverCaracteresNaoNumericos()
    {
        var query = new VisualizarCondominioQuery
        {
            Cnpj = "abc12.345xyz"
        };

        Assert.Equal("12345", query.Cnpj);
    }

    [Fact]
    public void NomeCondomDeveArmazenarValorInformado()
    {
        var query = new VisualizarCondominioQuery
        {
            NomeCondom = "Residencial Solar"
        };

        Assert.Equal("Residencial Solar", query.NomeCondom);
    }

    [Fact]
    public void PaginaDeveIniciarComValorPadraoUm()
    {
        var query = new VisualizarCondominioQuery();

        Assert.Equal(1, query.Pagina);
    }
}