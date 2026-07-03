using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuxiAPI.src.DTOs
{
    public class InformacoesCondominioDto
    {
    public string CodigoDoCondominio { get; set; } = string.Empty;
    public string CNPJDoCondominio { get; set; } = string.Empty;
    public string NomeDoCondominio { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string NumeroDoEndereco { get; set; } = string.Empty;
    public string EstadoDoEndereco { get; set; } = string.Empty;
    public string CidadeDoEndereco { get; set; } = string.Empty;
    public string BairroDoEndereco { get; set; } = string.Empty;
    public string CEPDoEndereco { get; set; } = string.Empty;
    public int NumeroDeTorres { get; set; }
    public int NumeroDeUnidades { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DataInicial_Administracao { get; set; } = string.Empty;
    public string DataFinal_Administracao { get; set; } = string.Empty;
    public string NomeGerenteDeContas { get; set; } = string.Empty;
    public string NomeSindico { get; set; } = string.Empty;
    }
}