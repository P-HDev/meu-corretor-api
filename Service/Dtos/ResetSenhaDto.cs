using System.ComponentModel.DataAnnotations;

namespace Service.Dtos;

public record ResetSenhaDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}

