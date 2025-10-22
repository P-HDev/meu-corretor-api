using System.ComponentModel.DataAnnotations;

namespace Service.Dtos;

public record LoginDto
{
    [Required] [EmailAddress] public string Email { get; init; } = string.Empty;
    [Required] public string Senha { get; init; } = string.Empty;
}