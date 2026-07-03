using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AuxiAPI.src.Services;
using AuxiAPI.src.DTOs;

namespace AuxiAPI.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CondominiosController(CondominioService service) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll([FromQuery] VisualizarCondominioQuery query)
        {
            var resultado = service.ListarCondominios(query); 
            return Ok(resultado);
        } 

        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] int id)
        {
            var resultado = service.ObterPorId(id);
            return Ok(resultado);
        }
    }
}