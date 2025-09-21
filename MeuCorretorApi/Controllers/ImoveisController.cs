using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace MeuCorretorApi.Controllers
{
    /// <summary>
    /// Endpoints para gestão de imóveis e suas imagens.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ImoveisController : ControllerBase
    {
        private readonly IImovelService _imovelService;

        public ImoveisController(IImovelService imovelService)
        {
            _imovelService = imovelService;
        }

        /// <summary>
        /// Lista todos os imóveis (apenas autenticado).
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ImovelDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ImovelDto>>> GetAll()
        {
            var imoveis = await _imovelService.GetAllAsync();
            return Ok(imoveis);
        }

        /// <summary>
        /// Obtém um imóvel pelo identificador interno (agora público/anonimo).
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ImovelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ImovelDto>> GetById(Guid id)
        {
            var imovel = await _imovelService.GetByIdAsync(id);
            if (imovel == null)
                return NotFound();
            return Ok(imovel);
        }

        /// <summary>
        /// Obtém imóvel público via PublicId (anônimo permitido).
        /// </summary>
        [HttpGet("public/{publicId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ImovelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ImovelDto>> GetByPublicId(string publicId)
        {
            var imovel = await _imovelService.GetByPublicIdAsync(publicId);
            if (imovel == null)
                return NotFound();
            return Ok(imovel);
        }

        /// <summary>
        /// Cria um novo imóvel informando URLs de imagens já hospedadas.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ImovelDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ImovelDto>> Create([FromBody] CreateImovelDto createImovelDto)
        {
            var created = await _imovelService.CreateAsync(createImovelDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Cria um novo imóvel enviando arquivos de imagem (upload multipart/form-data).
        /// </summary>
        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImovelDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ImovelDto>> CreateWithUpload([FromForm] CreateImovelUploadDto form)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var created = await _imovelService.CreateWithUploadAsync(form);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Atualiza um imóvel existente substituindo seus dados e URLs das imagens.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateImovelDto dto)
        {
            await _imovelService.UpdateAsync(id, dto);
            return NoContent();
        }

        /// <summary>
        /// Remove um imóvel.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {

            await _imovelService.DeleteAsync(id);
            return NoContent();
        }
    }
}