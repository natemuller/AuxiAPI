using AuxiAPI.src.DTOs;

namespace AuxiAPI.Tests.DTOsTest;

public class VisualizarCondominioQueryTest
{
    [Fact]
    public void Cnpj_DeveNormalizarCnpjComMascara()
    {
        var query = new VisualizarCondominioQuery
        {
            Cnpj = "12.345.678/0001-01"
        };

        Assert.Equal("12345678000101", query.Cnpj);
    }

    [Fact]
    public void Cnpj_DeveManterNull_QuandoValorForNull()
    {
        var query = new VisualizarCondominioQuery
        {
            Cnpj = null
        };

        Assert.Null(query.Cnpj);
    }

    [Fact]
    public void Cnpj_DeveRemoverCaracteresNaoNumericos()
    {
        var query = new VisualizarCondominioQuery
        {
            Cnpj = "abc12.345xyz"
        };

        Assert.Equal("12345", query.Cnpj);
    }

    [Fact]
    public void NomeCondom_DeveArmazenarValorInformado()
    {
        var query = new VisualizarCondominioQuery
        {
            NomeCondom = "Residencial Solar"
        };

        Assert.Equal("Residencial Solar", query.NomeCondom);
    }

    [Fact]
    public void Pagina_DeveIniciarComValorPadraoUm()
    {
        var query = new VisualizarCondominioQuery();

        Assert.Equal(1, query.Pagina);
    }
}