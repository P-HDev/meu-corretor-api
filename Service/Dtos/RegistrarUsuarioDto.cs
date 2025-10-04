using System.ComponentModel.DataAnnotations;

namespace Service.Dtos;

public record RegistrarUsuarioDto
{
    [Required] [MaxLength(150)] 
    public string Nome { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(180)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Senha { get; init; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Telefone { get; init; } = string.Empty;
}