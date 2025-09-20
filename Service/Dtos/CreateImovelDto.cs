using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Service.Dtos
{
    public class CreateImovelDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty;
        [Required]
        public string Endereco { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = string.Empty;
        [Required]
        public decimal Preco { get; set; }
        public int Area { get; set; }
        public int Quartos { get; set; }
        public int Banheiros { get; set; }
        public int Suites { get; set; }
        public int Vagas { get; set; }
        public ICollection<CreateImagemDto> Imagens { get; set; } = new List<CreateImagemDto>();
    }

    public class CreateImagemDto
    {
        [Required]
        public string Url { get; set; } = string.Empty;
    }
}