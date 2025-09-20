using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Service.Dtos
{
    // DTO específico para criação via multipart/form-data com upload de arquivos
    public class CreateImovelUploadDto
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

        // Arquivos de imagem enviados no formulário
        public List<IFormFile> Imagens { get; set; } = new();
    }
}

