using System.Collections.Generic;

namespace Service.Dtos
{
    public class ImovelDto
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
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
        public string CorretorTelefone { get; set; } = string.Empty;
        public ICollection<ImagemDto> Imagens { get; set; } = new List<ImagemDto>();
        public int? UserId { get; set; }
    }

    public class ImagemDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}