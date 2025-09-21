using System;

namespace Dominio
{
    public class Imagem
    {
        public Guid Id { get; private set; }
        public string Url { get; set; } = string.Empty;
        public Guid ImovelId { get; set; }
        public Imovel? Imovel { get; set; }

        public Imagem()
        {
            Id = Guid.NewGuid();
        }
    }
}
