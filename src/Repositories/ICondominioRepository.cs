using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuxiAPI.src.Entities;

namespace AuxiAPI.src.Repositories
{
    public interface ICondominioRepository
    {
        List<Condominio> LerTodos();
        Condominio? ObterPorId(int id);
    }
}