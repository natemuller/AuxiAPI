using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using System.Globalization;

namespace AuxiAPI.src.Mappers;

public static class AtlasUnidadeDtoMapper
{
    public static AtlasUnidadeDto ToDto(this AtlasUnidade unidade)
    {
        return new AtlasUnidadeDto
        {
            IdEconomia = unidade.IdEconomia,
            CodCondom = unidade.CodCondom,
            CodBloco = unidade.CodBloco ?? string.Empty,
            CodEconom = unidade.CodEconom ?? string.Empty,
            Fracao = unidade.Fracao,
            Ativa = unidade.Ativa ?? string.Empty,
            DataDesativa = FormatarDataDesativa(unidade.DataDesativa),
            DtAlteracao = unidade.DtAlteracao,
            TipoUnidade = unidade.TipoUnidade ?? string.Empty,
            CodCondomino = unidade.CodCondomino ?? string.Empty,
            NomeCondomino = unidade.NomeCondomino ?? string.Empty,
            EnderecoPrincipal = unidade.EnderecoPrincipal ?? string.Empty
        };
    }

    private static string FormatarDataDesativa(decimal? dataDesativa)
    {
        return dataDesativa?.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;
    }
}