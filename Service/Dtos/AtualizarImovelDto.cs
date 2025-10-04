namespace Service.Dtos;

public record AtualizarImovelDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Area { get; set; }
    public int Quartos { get; set; }
    public int Banheiros { get; set; }
    public int Suites { get; set; }
    public int Vagas { get; set; }
    public ICollection<AtualizarImagemDto> Imagens { get; set; } = new List<AtualizarImagemDto>();
}