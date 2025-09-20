using System.Threading.Tasks;
using Dominio;

namespace Dominio.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
    }
}

