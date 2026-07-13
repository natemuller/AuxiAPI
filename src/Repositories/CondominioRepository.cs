using AuxiAPI.src.Entities;
using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Common.Text;
using AuxiAPI.src.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.src.Repositories
{
    public class CondominioRepository(CondominiosDbContext context) : ICondominioRepository
    {
        public async Task<(List<Condominio> Itens, int TotalItens)> ListarAsync(
            VisualizarCondominioQuery filtro,
            int tamanhoPagina)
        {
            var query = context.AtlasCondominios
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.CodigoDoCondominio))
            {
                var codigoInformado = filtro.CodigoDoCondominio.Trim();

                if (!int.TryParse(codigoInformado, out var codCondom))
                    return (new List<Condominio>(), 0);

                query = query.Where(c => c.CodCondom == codCondom);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CNPJDoCondominio))
            {
                var cnpj = new string(
                    filtro.CNPJDoCondominio.Where(char.IsDigit).ToArray()
                );

                var cnpjMascarado = MascararCnpj(cnpj);

                query = query.Where(c =>
                    c.Cnpj == cnpj ||
                    c.Cnpj == cnpjMascarado
                );
            }

            if (!string.IsNullOrWhiteSpace(filtro.NomeDoCondominio))
            {
                var nomeNormalizado = TextNormalizer.NormalizarBusca(
                    filtro.NomeDoCondominio.Trim()
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

            var atlasItens = await query
                .OrderBy(c => c.CodCondom)
                .Skip(registrosParaPular)
                .Take(tamanhoPagina)
                .ToListAsync();

            var itens = atlasItens
                .Select(c => c.ToCondominio())
                .ToList();

            return (itens, totalItens);
        }

        public async Task<Condominio?> ObterPorIdAsync(int id)
        {
            var atlasCondominio = await context.AtlasCondominios
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CodCondom == id);

            return atlasCondominio?.ToCondominio();
        }

        private static string MascararCnpj(string cnpj)
        {
            if (cnpj.Length != 14)
                return cnpj;

            return Convert.ToUInt64(cnpj).ToString(@"00\.000\.000\/0000\-00");
        }
    }
}