using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO; 
using System.Text.Json; 
using AuxiAPI.src.Entities;
using AuxiAPI.src.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.src.Repositories
{
    public class CondominioRepository(CondominiosDbContext context): ICondominioRepository
    {
        private readonly string _caminhoJson = Path.Combine("Repositories", "Condominios.json");
        
        public List<Condominio> LerJson() 
        {
            return context.Condominios.AsNoTracking().ToList();
        }
        public Condominio? ObterPorId(int id)
        {
            return context.Condominios
            .AsNoTracking() 
            .SingleOrDefault(c => c.Id == id);
        }
    }
}