using AuxiAPI.src.Entities;
using AuxiAPI.src.Contexts;
using Microsoft.EntityFrameworkCore;
using AuxiAPI.src.DTOs;

namespace AuxiAPI.src.Repositories
{
    public class CondominioRepository(CondominiosDbContext context): ICondominioRepository
    {   
        public async Task<List<Condominio>> ListarAsync(VisualizarCondominioQuery filtro)
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
                var nome = filtro.NomeDoCondominio.Trim();
                query = query.Where(c =>
                    EF.Functions.ILike(
                        c.NomeDoCondominio,
                        $"%{nome}%"
                    )
                );
            }

            return await query.ToListAsync();
        }

        public async Task<Condominio?> ObterPorIdAsync(int id)
        {
            return await context.Condominios
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}