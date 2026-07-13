namespace AuxiAPI.src.Entities;

public class AtlasCondominio
{
    public int CodCondom { get; set; }

    public string? NomeCondom { get; set; }

    public string? Ativo { get; set; }

    public string? Cnpj { get; set; }

    public string? Cei { get; set; }

    public string? InscrMunicip { get; set; }

    public int? QtdBlocos { get; set; }

    public int? QtdUnidades { get; set; }

    public decimal? TotalFracao { get; set; }

    public int? DiaVencDoc { get; set; }

    public int? DataInicioAdm { get; set; }

    public int? DataDistrato { get; set; }

    public string? MotivoDistrato { get; set; }

    public string? Assessor { get; set; }

    public string? Filial { get; set; }

    public string? Agencia { get; set; }

    public string? Sindico { get; set; }

    public string? SubSindico { get; set; }

    public string? Conselheiro { get; set; }

    public string? Gestor { get; set; }

    public string? ConselhoFiscal { get; set; }

    public string? ConselhoConsultivo { get; set; }

    public string? ConselhoSuplente { get; set; }

    public string? TipoCondominio { get; set; }

    public string? TipoCategoria { get; set; }

    public DateTime? DtAlteracao { get; set; }

    public string? TipoLograd { get; set; }

    public string? Lograd { get; set; }

    public string? Numero { get; set; }

    public string? Bairro { get; set; }

    public string? Cidade { get; set; }

    public string? Cep8Log { get; set; }

    public string? Uf { get; set; }

    public string? CodPessoaSindico { get; set; }

    public string? NomeSindico { get; set; }

    public string? CpfDocnpj { get; set; }

    public string? CondGarantido { get; set; }

    public string? TipoConta { get; set; }

    public string? ObsCobranca { get; set; }

    public string? Garantidora { get; set; }

    public List<AtlasBloco> Blocos { get; set; } = [];

    public List<AtlasUnidade> Unidades { get; set; } = [];
}