using System.ComponentModel.DataAnnotations;

namespace Service.Dtos;

public record CriarImovelDto
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
    public ICollection<CriarImagemDto> Imagens { get; init; } = new List<CriarImagemDto>();
}