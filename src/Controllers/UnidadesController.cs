using Microsoft.AspNetCore.Mvc;
using AuxiAPI.src.Services;
using AuxiAPI.src.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace AuxiAPI.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnidadesController(UnidadeService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] VisualizarUnidadeQuery query)
        {
            var resultado = await service.ListarUnidadesAsync(query);
            return Ok(resultado);
        }

        [HttpGet("{ideconomia:int}")]
        public async Task<IActionResult> GetByIdEconomia([FromRoute] int ideconomia)
        {
            var resultado = await service.ObterPorIdEconomiaAsync(ideconomia);
            return Ok(resultado);
        }
    }
}