using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuxiAPI.src.Entities;

namespace AuxiAPI.src.DTOs
{
    public class VisualizarCondominioQuery
    {
    public string? CodigoDoCondominio { get; set; }

    private string? _cnpjDoCondominio;
    public string? CNPJDoCondominio
    {
        get => _cnpjDoCondominio;
        set => _cnpjDoCondominio = value != null 
            ? new string(value.Where(char.IsDigit).ToArray()) 
                : null;
    }
    public string? NomeDoCondominio { get; set; }
    }
}