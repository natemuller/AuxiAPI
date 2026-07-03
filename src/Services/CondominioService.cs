using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Repositories;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Common;

namespace AuxiAPI.src.Services
{
    public class CondominioService (ICondominioRepository repository)
    {
        public List<InformacoesCondominioDto> ListarCondominios(VisualizarCondominioQuery query)
        {
            if (query.CodigoDoCondominio?.Length > 15)
                throw new ArgumentException(MensagensDeErro.CodigoTamanhoExcedido);

            if (query.CNPJDoCondominio?.Length > 15)
                throw new ArgumentException(MensagensDeErro.CnpjTamanhoExcedido);

            if (query.NomeDoCondominio?.Length > 200)
                throw new ArgumentException(MensagensDeErro.NomeTamanhoExcedido);

            var condominios = repository.LerTodos();

            if (!string.IsNullOrEmpty(query.CodigoDoCondominio))
                condominios = condominios.Where(c => c.CodigoDoCondominio == query.CodigoDoCondominio).ToList();

            if (!string.IsNullOrEmpty(query.CNPJDoCondominio))
                condominios = condominios.Where(c => c.CNPJDoCondominio == query.CNPJDoCondominio).ToList();

            if (!string.IsNullOrEmpty(query.NomeDoCondominio))
                condominios = condominios.Where(c => c.NomeDoCondominio.Contains(query.NomeDoCondominio, System.StringComparison.OrdinalIgnoreCase)).ToList();

            return condominios.Select(MapearParaDto).ToList();
        }

        public InformacoesCondominioDto? ObterPorId(int id)
        {
            var condominio = repository.ObterPorId(id) ?? throw new KeyNotFoundException($"condominio com ID {id} nao foi encontrado.");
            return MapearParaDto(condominio);
        }

        private static InformacoesCondominioDto MapearParaDto(Condominio condominio)
        {
            return new InformacoesCondominioDto
            {
                CodigoDoCondominio = condominio.CodigoDoCondominio,
                CNPJDoCondominio = condominio.CNPJDoCondominio,
                NomeDoCondominio = condominio.NomeDoCondominio,
                Endereco = condominio.Endereco,
                NumeroDoEndereco = condominio.NumeroDoEndereco,
                EstadoDoEndereco = condominio.EstadoDoEndereco,
                CidadeDoEndereco = condominio.CidadeDoEndereco,
                BairroDoEndereco = condominio.BairroDoEndereco,
                CEPDoEndereco = condominio.CEPDoEndereco,
                NumeroDeTorres = condominio.NumeroDeTorres,
                NumeroDeUnidades = condominio.NumeroDeUnidades,
                Status = condominio.Status,
                DataInicial_Administracao = condominio.DataInicial_Administracao,
                DataFinal_Administracao = condominio.DataFinal_Administracao,
                NomeGerenteDeContas = condominio.NomeGerenteDeContas,
                NomeSindico = condominio.NomeSindico
            };
        }
    }
}