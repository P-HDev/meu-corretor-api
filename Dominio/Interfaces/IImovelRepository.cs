using System.Collections.Generic;
using System.Threading.Tasks;
using Dominio;

namespace Dominio.Interfaces
{
    public interface IImovelRepository
    {
        Task<IEnumerable<Imovel>> GetAllAsync();
        Task<IEnumerable<Imovel>> GetAllByUserAsync(int userId);
        Task<Imovel?> GetByIdAsync(int id);
        Task<Imovel?> GetByPublicIdAsync(string publicId);
        Task<Imovel> AddAsync(Imovel imovel);
        Task UpdateAsync(Imovel imovel);
        Task DeleteAsync(int id);
    }
}
