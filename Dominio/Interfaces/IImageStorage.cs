using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dominio.Interfaces
{
    public interface IImageStorage
    {
        Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);
    }
}

