namespace Dominio;

public class Imovel
{
    private readonly List<string> _imagensUrls = new();
    public Guid Id { get; private set; }
    public string PublicId { get; private set; } = string.Empty;
    public string Titulo { get; private set; } = string.Empty;
    public string Endereco { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int Area { get; private set; }
    public int Quartos { get; private set; }
    public int Banheiros { get; private set; }
    public int Suites { get; private set; }
    public int Vagas { get; private set; }
    public string CorretorTelefone { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public IReadOnlyCollection<string> ImagensUrls => _imagensUrls.AsReadOnly();

    protected Imovel()
    {
    }

    internal Imovel(string titulo, string endereco, string descricao, string status, decimal preco,
        int area, int quartos, int banheiros, int suites, int vagas, string corretorTelefone)
    {
        Id = Guid.NewGuid();
        PublicId = Guid.NewGuid().ToString("N");
        DefinirDadosBasicos(titulo, endereco, descricao, status, preco, area, quartos, banheiros, suites, vagas);
        DefinirCorretorTelefone(corretorTelefone);
    }

    public void Atualizar(string titulo, string endereco, string descricao, string status, decimal preco,
        int area, int quartos, int banheiros, int suites, int vagas)
    {
        DefinirDadosBasicos(titulo, endereco, descricao, status, preco, area, quartos, banheiros, suites, vagas);
    }

    public void DefinirCorretorTelefone(string telefone)
    {
        CorretorTelefone = (telefone ?? string.Empty).Trim();
    }

    public void AdicionarImagemUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        _imagensUrls.Add(url);
    }

    public void RemoverImagemUrl(string url)
    {
        _imagensUrls.Remove(url);
    }

    public void LimparImagens()
    {
        _imagensUrls.Clear();
    }

    public void DefinirImagens(IEnumerable<string> urls)
    {
        _imagensUrls.Clear();
        foreach (var url in urls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            _imagensUrls.Add(url);
        }
    }

    public void DefinirUserId(Guid? userId)
    {
        UserId = userId;
    }

    private void DefinirDadosBasicos(string titulo, string endereco, string descricao, string status, decimal preco,
        int area, int quartos, int banheiros, int suites, int vagas)
    {
        if (string.IsNullOrWhiteSpace(titulo)) throw new ArgumentException("Título é obrigatório", nameof(titulo));
        if (string.IsNullOrWhiteSpace(endereco))
            throw new ArgumentException("Endereço é obrigatório", nameof(endereco));
        if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Status é obrigatório", nameof(status));
        if (preco < 0) throw new ArgumentException("Preço não pode ser negativo", nameof(preco));

        Titulo = titulo.Trim();
        Endereco = endereco.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
        Status = status.Trim();
        Preco = preco;
        Area = area;
        Quartos = quartos;
        Banheiros = banheiros;
        Suites = suites;
        Vagas = vagas;
    }
}