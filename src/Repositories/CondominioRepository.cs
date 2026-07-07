using AuxiAPI.src.Entities;
using AuxiAPI.src.Contexts;
using Microsoft.EntityFrameworkCore;
using AuxiAPI.src.DTOs;
using AuxiAPI.src.Common.Text;

namespace AuxiAPI.src.Repositories
{
    public class CondominioRepository(CondominiosDbContext context): ICondominioRepository
    {   
        public async Task<(List<Condominio> Itens, int TotalItens)> ListarAsync(VisualizarCondominioQuery filtro, int tamanhoPagina)
        {
            var query = context.Condominios
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.CodigoDoCondominio))
            {
                var codigo = filtro.CodigoDoCondominio.PadLeft(4, '0');

                query = query.Where(c =>
                    c.CodigoDoCondominio == codigo
                );
            }

            if (!string.IsNullOrWhiteSpace(filtro.CNPJDoCondominio))
            {
                var cnpj = new string(
                    [.. filtro.CNPJDoCondominio.Where(char.IsDigit)]
                );

                query = query.Where(c =>
                    c.CNPJDoCondominio == cnpj
                );
            }

            if (!string.IsNullOrWhiteSpace(filtro.NomeDoCondominio))
            {
                var nomeNormalizado = TextNormalizer.NormalizarBusca(filtro.NomeDoCondominio.Trim());

                query = query.Where(c =>
                    EF.Functions.ILike(
                        PostgresDbFunctions.Unaccent(c.NomeDoCondominio),
                        $"%{nomeNormalizado}%"
                    )
                );
            }

            var totalItens = await query.CountAsync();

            var registrosParaPular = (filtro.Pagina - 1) * tamanhoPagina;

            var itens = await query
                .OrderBy(c => c.Id)
                .Skip(registrosParaPular)
                .Take(tamanhoPagina)
            .ToListAsync();

            return (itens, totalItens);
        }   

        public async Task<Condominio?> ObterPorIdAsync(int id)
        {
            return await context.Condominios
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}