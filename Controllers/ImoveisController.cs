
using System.Collections.Generic;
using System.Threading.Tasks;
using Dominio.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Service.Dtos;

namespace MeuCorretorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImoveisController : ControllerBase
    {
        private readonly IImovelService _imovelService;

        public ImoveisController(IImovelService imovelService)
        {
            _imovelService = imovelService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImovelDto>>> GetAll()
        {
            var imoveis = await _imovelService.GetAllAsync();
            return Ok(imoveis);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImovelDto>> GetById(int id)
        {
            var imovel = await _imovelService.GetByIdAsync(id);
            if (imovel == null)
            {
                return NotFound();
            }
            return Ok(imovel);
        }

        [HttpPost]
        public async Task<ActionResult<ImovelDto>> Create(CreateImovelDto createImovelDto)
        {
            var createdImovel = await _imovelService.CreateAsync(createImovelDto);
            return CreatedAtAction(nameof(GetById), new { id = createdImovel.Id }, createdImovel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateImovelDto updateImovelDto)
        {
            await _imovelService.UpdateAsync(id, updateImovelDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _imovelService.DeleteAsync(id);
            return NoContent();
        }
    }
}
