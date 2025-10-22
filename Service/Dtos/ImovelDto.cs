namespace Service.Dtos;

public record ImovelDto
{
    public Guid Id { get; init; }
    public string PublicId { get; init; } = string.Empty;
    public string ShareUrl { get; init; } = string.Empty;
    public string Titulo { get; init; } = string.Empty;
    public string Endereco { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal Preco { get; init; }
    public int Area { get; init; }
    public int Quartos { get; init; }
    public int Banheiros { get; init; }
    public int Suites { get; init; }
    public int Vagas { get; init; }
    public string CorretorTelefone { get; init; } = string.Empty;
    public List<string> ImagensUrls { get; init; } = new();
    public Guid? UserId { get; init; }
}