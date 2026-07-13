using AuxiAPI.src.Entities;

namespace AuxiAPI.src.Mappers;

public static class AtlasCondominioMapper
{
    public static Condominio ToCondominio(this AtlasCondominio atlas)
    {
        return new Condominio
        {
            Id = atlas.CodCondom,

            CodigoDoCondominio = atlas.CodCondom.ToString("D4"),
            CNPJDoCondominio = atlas.Cnpj ?? string.Empty,
            NomeDoCondominio = atlas.NomeCondom ?? string.Empty,

            Endereco = atlas.Lograd ?? string.Empty,
            NumeroDoEndereco = atlas.Numero ?? string.Empty,
            EstadoDoEndereco = atlas.Uf ?? string.Empty,
            CidadeDoEndereco = atlas.Cidade ?? string.Empty,
            BairroDoEndereco = atlas.Bairro ?? string.Empty,
            CEPDoEndereco = ApenasNumeros(atlas.Cep8Log),

            NumeroDeTorres = atlas.QtdBlocos ?? 0,
            NumeroDeUnidades = atlas.QtdUnidades ?? 0,

            Status = ConverterStatus(atlas.Ativo),

            DataInicial_Administracao = ConverterInteiroParaTexto(atlas.DataInicioAdm),
            DataFinal_Administracao = ConverterInteiroParaTexto(atlas.DataDistrato),

            NomeGerenteDeContas = atlas.Assessor ?? string.Empty,
            NomeSindico = atlas.NomeSindico ?? string.Empty
        };
    }

    private static string ConverterStatus(string? ativo)
    {
        return ativo?.Trim().ToUpperInvariant() switch
        {
            "S" => "Ativo",
            "N" => "Inativo",
            _ => string.Empty
        };
    }

    private static string ConverterInteiroParaTexto(int? valor)
    {
        return valor?.ToString() ?? string.Empty;
    }

    private static string ApenasNumeros(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        return new string(valor.Where(char.IsDigit).ToArray());
    }
}