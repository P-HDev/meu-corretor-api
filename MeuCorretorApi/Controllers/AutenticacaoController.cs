using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Dtos;

namespace MeuCorretorApi.Controllers;
   
[ApiController]
[Route("api/[controller]")]
public class AutenticacaoController(IAutenticacaoService autenticacaoService) : ControllerBase
{
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(RespostaAutenticacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegistrarUsuarioDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var resp = await autenticacaoService.RegisterAsync(dto);
            return Ok(resp);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespostaAutenticacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var resp = await autenticacaoService.LoginAsync(dto);
            return Ok(resp);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Credenciais inválidas" });
        }
    }
}