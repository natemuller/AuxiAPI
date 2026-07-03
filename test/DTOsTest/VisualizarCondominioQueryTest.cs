using AuxiAPI.src.DTOs;

namespace AuxiAPI.Tests.DTOsTest;

public class VisualizarCondominioQueryTest
{
    [Fact]
    public void CNPJDoCondominio_DeveNormalizarCnpjComMascara()
    {
        var query = new VisualizarCondominioQuery { CNPJDoCondominio = "12.345.678/0001-01" };

        Assert.Equal("12345678000101", query.CNPJDoCondominio);
    }

    [Fact]
    public void CNPJDoCondominio_DeveManterNull_QuandoValorForNull()
    {
        var query = new VisualizarCondominioQuery { CNPJDoCondominio = null };

        Assert.Null(query.CNPJDoCondominio);
    }

    [Fact]
    public void CNPJDoCondominio_DeveRemoverCaracteresNaoNumericos()
    {
        var query = new VisualizarCondominioQuery { CNPJDoCondominio = "abc12.345xyz" };

        Assert.Equal("12345", query.CNPJDoCondominio);
    }
}
