using AuxiAPI.src.Contexts;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Entities;
using AuxiAPI.src.Common.Text;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.src.Repositories
{
    public class UnidadeRepository(CondominiosDbContext context) : IUnidadeRepository
    {
        public async Task<(List<AtlasUnidade> Itens, int TotalItens)> ListarAsync(
            VisualizarUnidadeQuery filtro,
            int tamanhoPagina)
        {
            var query = context.AtlasUnidades
                .AsNoTracking()
                .AsQueryable();

            if (filtro.CodCondom.HasValue)
            {
                query = query.Where(u => u.CodCondom == filtro.CodCondom.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtro.NomeCondomino))
            {
                var nomeNormalizado = TextNormalizer.NormalizarBusca(
                    filtro.NomeCondomino.Trim()
                );

                query = query.Where(u =>
                    u.NomeCondomino != null &&
                    EF.Functions.ILike(
                        PostgresDbFunctions.Unaccent(u.NomeCondomino),
                        $"%{nomeNormalizado}%"
                    )
                );
            }

            var totalItens = await query.CountAsync();

            var registrosParaPular = (filtro.Pagina - 1) * tamanhoPagina;

            var itens = await query
                .OrderBy(u => u.IdEconomia)
                .Skip(registrosParaPular)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (itens, totalItens);
        }

        public async Task<AtlasUnidade?> ObterPorIdEconomiaAsync(int idEconomia)
        {
            return await context.AtlasUnidades
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdEconomia == idEconomia);
        }
    }
}