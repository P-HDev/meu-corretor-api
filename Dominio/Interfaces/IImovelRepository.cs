using System.Collections.Generic;
using System.Threading.Tasks;
using Dominio;

namespace Dominio.Interfaces
{
    public interface IImovelRepository
    {
        Task<IEnumerable<Imovel>> GetAllAsync();
        Task<IEnumerable<Imovel>> GetAllByUserAsync(Guid userId);
        Task<Imovel?> GetByIdAsync(Guid id);
        Task<Imovel?> GetByPublicIdAsync(string publicId);
        Task<Imovel> AddAsync(Imovel imovel);
        Task UpdateAsync(Imovel imovel);
        Task DeleteAsync(Guid id);
    }
}
