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
        public async Task<IActionResult> GetAll([FromQuery] VisualizarCondominioQuery query)
        {
            var resultado = await service.ListarCondominiosAsync(query);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var resultado = await service.ObterPorIdAsync(id);
            return Ok(resultado);
        }
    }
}