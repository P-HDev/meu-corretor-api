namespace Dominio.Interfaces;

public interface IImageStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);
}