using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Dtos;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace MeuCorretorApi.Controllers
{
    /// <summary>
    /// Autenticação e registro de usuários.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>
        /// Registra um novo usuário.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var resp = await _authService.RegisterAsync(dto);
                return Ok(resp);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Efetua login e retorna token JWT.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var resp = await _authService.LoginAsync(dto);
                return Ok(resp);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Credenciais inválidas" });
            }
        }
    }
}

