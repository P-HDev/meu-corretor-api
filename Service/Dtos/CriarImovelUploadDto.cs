using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Service.Dtos;

public record CriarImovelUploadDto
{
    [Required]
    public string Titulo { get; init; } = string.Empty;
    [Required]
    public string Endereco { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    [Required]
    public string Status { get; init; } = string.Empty;
    [Required]
    public decimal Preco { get; init; }
    public int Area { get; init; }
    public int Quartos { get; init; }
    public int Banheiros { get; init; }
    public int Suites { get; init; }
    public int Vagas { get; init; }

    public List<IFormFile> Imagens { get; init; } = new();
}