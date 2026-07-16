namespace AuxiAPI.src.DTOs;

public class AtlasUnidadeDto
{
    public int IdEconomia {get; set;}
    public int CodCondom {get; set;}
    public string CodBloco {get; set;} = string.Empty;
    public string CodEconom {get; set;} = string.Empty;
    public decimal? Fracao {get; set;}
    public string? Ativa {get; set;} = string.Empty;
    public string? DataDesativa {get; set;} = string.Empty;
    public DateTime? DtAlteracao {get; set;}
    public string? TipoUnidade {get; set;} = string.Empty;
    public string? CodCondomino {get; set;} = string.Empty;
    public string? NomeCondomino {get; set;} = string.Empty;
    public string? EnderecoPrincipal {get; set;} = string.Empty;
}