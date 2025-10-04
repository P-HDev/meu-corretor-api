using Dominio;
using Dominio.Interfaces;
using InfraEstrutura.ContextoBancoPsql;
using Microsoft.EntityFrameworkCore;

namespace InfraEstrutura.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ContextoDb _context;
        public UserRepository(ContextoDb context) => _context = context;

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}