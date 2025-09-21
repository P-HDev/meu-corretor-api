using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dominio;
using Dominio.Interfaces;
using InfraEstrutura.ContextoBancoPsql;
using Microsoft.EntityFrameworkCore;

namespace InfraEstrutura.Repositories
{
    public class ImovelRepository : IImovelRepository
    {
        private readonly ContextoDb _context;

        public ImovelRepository(ContextoDb context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Imovel>> GetAllAsync()
        {
            return await _context.Imoveis.Include(i => i.Imagens).ToListAsync();
        }

        public async Task<Imovel?> GetByIdAsync(Guid id)
        {
            return await _context.Imoveis.Include(i => i.Imagens).FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Imovel?> GetByPublicIdAsync(string publicId)
        {
            return await _context.Imoveis.Include(i => i.Imagens).FirstOrDefaultAsync(i => i.PublicId == publicId);
        }

        public async Task<Imovel> AddAsync(Imovel imovel)
        {
            _context.Imoveis.Add(imovel);
            await _context.SaveChangesAsync();
            return imovel;
        }

        public async Task UpdateAsync(Imovel imovel)
        {
            _context.Entry(imovel).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var imovel = await _context.Imoveis.FindAsync(id);
            if (imovel != null)
            {
                _context.Imoveis.Remove(imovel);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Imovel>> GetAllByUserAsync(Guid userId)
        {
            return await _context.Imoveis.Include(i => i.Imagens)
                .Where(i => i.UserId.HasValue && i.UserId.Value == userId)
                .ToListAsync();
        }
    }
}
