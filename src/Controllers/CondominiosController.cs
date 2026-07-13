using Microsoft.AspNetCore.Mvc;
using AuxiAPI.src.Services;
using AuxiAPI.src.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace AuxiAPI.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CondominiosController(CondominioService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] VisualizarCondominioQuery query)
        {
            var resultado = await service.ListarCondominiosAsync(query);
            return Ok(resultado);
        }

        [HttpGet("{codcondom:int}")]
        public async Task<IActionResult> GetByCodCondom([FromRoute] int codcondom)
        {
            var resultado = await service.ObterPorCodCondomAsync(codcondom);
            return Ok(resultado);
        }
    }
}