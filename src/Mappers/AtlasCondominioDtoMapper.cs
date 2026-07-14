using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;

namespace AuxiAPI.src.Mappers;

public static class AtlasCondominioDtoMapper
{
    public static AtlasCondominioDto ToDto(this AtlasCondominio atlas)
    {
        return new AtlasCondominioDto
        {
            CodCondom = atlas.CodCondom,
            NomeCondom = atlas.NomeCondom,
            Ativo = atlas.Ativo,
            Cnpj = atlas.Cnpj,
            Cei = atlas.Cei,
            InscrMunicip = atlas.InscrMunicip,
            QtdBlocos = atlas.QtdBlocos,
            QtdUnidades = atlas.QtdUnidades,
            TotalFracao = atlas.TotalFracao,
            DiaVencDoc = atlas.DiaVencDoc,
            DataInicioAdm = atlas.DataInicioAdm,
            DataDistrato = atlas.DataDistrato,
            MotivoDistrato = atlas.MotivoDistrato,
            Assessor = atlas.Assessor,
            Filial = atlas.Filial,
            Agencia = atlas.Agencia,
            Sindico = atlas.Sindico,
            SubSindico = atlas.SubSindico,
            Conselheiro = atlas.Conselheiro,
            Gestor = atlas.Gestor,
            ConselhoFiscal = atlas.ConselhoFiscal,
            ConselhoConsultivo = atlas.ConselhoConsultivo,
            ConselhoSuplente = atlas.ConselhoSuplente,
            TipoCondominio = atlas.TipoCondominio,
            TipoCategoria = atlas.TipoCategoria,
            DtAlteracao = atlas.DtAlteracao,
            TipoLograd = atlas.TipoLograd,
            Lograd = atlas.Lograd,
            Numero = atlas.Numero,
            Bairro = atlas.Bairro,
            Cidade = atlas.Cidade,
            Cep8Log = atlas.Cep8Log,
            Uf = atlas.Uf,
            CodPessoaSindico = atlas.CodPessoaSindico,
            NomeSindico = atlas.NomeSindico,
            CpfDocnpj = atlas.CpfDocnpj,
            CondGarantido = atlas.CondGarantido,
            TipoConta = atlas.TipoConta,
            ObsCobranca = atlas.ObsCobranca,
            Garantidora = atlas.Garantidora
        };
    }
}