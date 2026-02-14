using Service.Dtos;

namespace Service.Interfaces;

public interface IAutenticacaoService
{
    Task<RespostaAutenticacaoDto> RegistrarAsync(RegistrarUsuarioDto dto);
    Task<RespostaAutenticacaoDto> LoginAsync(LoginDto dto);
    Task ResetarSenhaAsync(string email);
}