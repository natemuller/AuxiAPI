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
        public List<Condominio> LerTodos() 
        {
            return [.. context.Condominios.AsNoTracking()];
        }
        public Condominio? ObterPorId(int id)
        {
            return context.Condominios
            .AsNoTracking() 
            .SingleOrDefault(c => c.Id == id);
        }
    }
}