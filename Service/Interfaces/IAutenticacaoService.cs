using Service.Dtos;

namespace Service.Interfaces;

public interface IAutenticacaoService
{
    Task<RespostaAutenticacaoDto> RegisterAsync(RegistrarUsuarioDto dto);
    Task<RespostaAutenticacaoDto> LoginAsync(LoginDto dto);
}