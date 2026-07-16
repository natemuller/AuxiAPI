using AuxiAPI.src.DTOs;
using Xunit;

namespace AuxiAPI.Test.DTOsTest
{
    public class VisualizarUnidadeQueryTest
    {
        [Fact]
        public void QuandoCriarQuery_DeveIniciarPaginaComValorPadraoUm()
        {
            var query = new VisualizarUnidadeQuery();

            Assert.Equal(1, query.Pagina);
        }

        [Fact]
        public void DevePermitirFiltroPorCodCondom()
        {
            var query = new VisualizarUnidadeQuery
            {
                CodCondom = 5396
            };

            Assert.Equal(5396, query.CodCondom);
        }

        [Fact]
        public void DevePermitirFiltroPorNomeCondomino()
        {
            var query = new VisualizarUnidadeQuery
            {
                NomeCondomino = "João Silva"
            };

            Assert.Equal("João Silva", query.NomeCondomino);
        }

        [Fact]
        public void DevePermitirCombinarCodCondomNomeCondominoEPagina()
        {
            var query = new VisualizarUnidadeQuery
            {
                CodCondom = 5396,
                NomeCondomino = "João",
                Pagina = 2
            };

            Assert.Equal(5396, query.CodCondom);
            Assert.Equal("João", query.NomeCondomino);
            Assert.Equal(2, query.Pagina);
        }
    }
}