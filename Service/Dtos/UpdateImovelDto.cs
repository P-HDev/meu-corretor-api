using System.Collections.Generic;

namespace Service.Dtos
{
    public class UpdateImovelDto
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
        public ICollection<UpdateImagemDto> Imagens { get; set; } = new List<UpdateImagemDto>();
    }

    public class UpdateImagemDto
    {
        public Guid? Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}