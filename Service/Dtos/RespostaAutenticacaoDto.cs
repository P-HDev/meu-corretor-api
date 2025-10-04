namespace Service.Dtos;

public record RespostaAutenticacaoDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public UsuarioDto User { get; init; } = new();
}