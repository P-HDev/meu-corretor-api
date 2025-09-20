
namespace Dominio
{
    public class Imagem
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int ImovelId { get; set; }
        public Imovel? Imovel { get; set; }
    }
}
