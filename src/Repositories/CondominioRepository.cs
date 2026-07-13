using AuxiAPI.src.Entities;
using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Common.Text;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.src.Repositories
{
    public class CondominioRepository(CondominiosDbContext context) : ICondominioRepository
    {
        public async Task<(List<AtlasCondominio> Itens, int TotalItens)> ListarAsync(
            VisualizarCondominioQuery filtro,
            int tamanhoPagina)
        {
            var query = context.AtlasCondominios
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Cnpj))
            {
                var cnpj = new string(
                    filtro.Cnpj.Where(char.IsDigit).ToArray()
                );

                var cnpjMascarado = MascararCnpj(cnpj);

                query = query.Where(c =>
                    c.Cnpj == cnpj ||
                    c.Cnpj == cnpjMascarado
                );
            }

            if (!string.IsNullOrWhiteSpace(filtro.NomeCondom))
            {
                var nomeNormalizado = TextNormalizer.NormalizarBusca(
                    filtro.NomeCondom.Trim()
                );

                query = query.Where(c =>
                    c.NomeCondom != null &&
                    EF.Functions.ILike(
                        PostgresDbFunctions.Unaccent(c.NomeCondom),
                        $"%{nomeNormalizado}%"
                    )
                );
            }

            var totalItens = await query.CountAsync();

            var registrosParaPular = (filtro.Pagina - 1) * tamanhoPagina;

            var itens = await query
                .OrderBy(c => c.CodCondom)
                .Skip(registrosParaPular)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (itens, totalItens);
        }

        public async Task<AtlasCondominio?> ObterPorCodCondomAsync(int codcondom)
        {
            return await context.AtlasCondominios
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CodCondom == codcondom);
        }

        private static string MascararCnpj(string cnpj)
        {
            if (cnpj.Length != 14)
                return cnpj;

            return Convert.ToUInt64(cnpj).ToString(@"00\.000\.000\/0000\-00");
        }
    }
}