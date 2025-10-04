namespace Dominio.Builders;

    public class ImovelBuilder
    {
        private string _titulo = string.Empty;
        private string _endereco = string.Empty;
        private string _descricao = string.Empty;
        private string _status = string.Empty;
        private decimal _preco;
        private int _area;
        private int _quartos;
        private int _banheiros;
        private int _suites;
        private int _vagas;
        private string _corretorTelefone = string.Empty;
        private readonly List<string> _imagens = new();

        public static ImovelBuilder Novo() => new();

        public ImovelBuilder ComTitulo(string valor) { _titulo = valor; return this; }
        public ImovelBuilder ComEndereco(string valor) { _endereco = valor; return this; }
        public ImovelBuilder ComDescricao(string valor) { _descricao = valor; return this; }
        public ImovelBuilder ComStatus(string valor) { _status = valor; return this; }
        public ImovelBuilder ComPreco(decimal valor) { _preco = valor; return this; }
        public ImovelBuilder ComArea(int valor) { _area = valor; return this; }
        public ImovelBuilder ComQuartos(int valor) { _quartos = valor; return this; }
        public ImovelBuilder ComBanheiros(int valor) { _banheiros = valor; return this; }
        public ImovelBuilder ComSuites(int valor) { _suites = valor; return this; }
        public ImovelBuilder ComVagas(int valor) { _vagas = valor; return this; }
        public ImovelBuilder ComCorretorTelefone(string valor) { _corretorTelefone = valor; return this; }
        public ImovelBuilder AdicionarImagem(string url) { if(!string.IsNullOrWhiteSpace(url)) _imagens.Add(url); return this; }
        public ImovelBuilder AdicionarImagens(IEnumerable<string> urls) { if (urls!=null) _imagens.AddRange(urls.Where(u=>!string.IsNullOrWhiteSpace(u))); return this; }

        public Imovel Build()
        {
            var imovel = new Imovel(_titulo, _endereco, _descricao, _status, _preco, _area, _quartos, _banheiros, _suites, _vagas, _corretorTelefone);
            foreach (var img in _imagens) imovel.AdicionarImagem(img);
            return imovel;
        }
    }