namespace AuxiAPI.src.Entities;

public class AtlasBloco
{
    public int CodCondom { get; set; }

    public string CodBloco { get; set; } = string.Empty;

    public string? CodBlocoBase { get; set; }

    public string? Descricao { get; set; }

    public int? QtdEconomias { get; set; }

    public string? TipoLograd { get; set; }

    public string? Lograd { get; set; }

    public string? Numero { get; set; }

    public string? Bairro { get; set; }

    public string? Cidade { get; set; }

    public string? Uf { get; set; }

    public string? Cep8Log { get; set; }

    public string? Ativo { get; set; }

    public string? TipoBloco { get; set; }

    public AtlasCondominio? Condominio { get; set; }

    public List<AtlasUnidade> Unidades { get; set; } = [];
}