namespace AuxiAPI.src.Entities;

public class AtlasUnidade
{
    public int IdEconomia { get; set; }

    public int CodCondom { get; set; }

    public string CodBloco { get; set; } = string.Empty;

    public string? CodEconom { get; set; }

    public decimal? Fracao { get; set; }

    public string? Ativa { get; set; }

    public decimal? DataDesativa { get; set; }

    public DateTime? DtAlteracao { get; set; }

    public string? TipoUnidade { get; set; }

    public string? CodCondomino { get; set; }

    public string? NomeCondomino { get; set; }

    public string? EnderecoPrincipal { get; set; }

    public string? EnderecoCorrespondencia { get; set; }

    public string? EnderecoCobranca { get; set; }

    public string? CodPesDebConta { get; set; }

    public string? NomeDebConta { get; set; }

    public string? CodFornec { get; set; }

    public string? CodNaAdmDest { get; set; }

    public string? CodFornecEscrit { get; set; }

    public AtlasCondominio? Condominio { get; set; }

    public AtlasBloco? Bloco { get; set; }
}