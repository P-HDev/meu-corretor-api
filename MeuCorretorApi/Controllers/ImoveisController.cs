using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace MeuCorretorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImoveisController(IImovelService imovelService) : ControllerBase
{
    private readonly IImovelService _imovelService = imovelService ?? throw new ArgumentNullException(nameof(imovelService));

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ImovelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ImovelDto>>> GetAll()
    {
        var imoveis = await _imovelService.GetAllAsync();
        return Ok(imoveis);
    }

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

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ImovelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImovelDto>> Create([FromBody] CriarImovelDto criarImovelDto)
    {
        var created = await _imovelService.CreateAsync(criarImovelDto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImovelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImovelDto>> CreateWithUpload([FromForm] CriarImovelUploadDto form)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var created = await _imovelService.CreateWithUploadAsync(form);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarImovelDto dto)
    {
        await _imovelService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _imovelService.DeleteAsync(id);
        return NoContent();
    }
}