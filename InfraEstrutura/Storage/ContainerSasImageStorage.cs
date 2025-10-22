using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dominio.Interfaces;

namespace InfraEstrutura.Storage;

public class ContainerSasImageStorage : IImageStorage
{
    private readonly BlobContainerClient _containerClient;

    public ContainerSasImageStorage(BlobContainerClient containerClient)
    {
        _containerClient = containerClient;
    }

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extension)) extension = ".jpg";
        if (!extension.StartsWith('.')) extension = "." + extension;

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var blobClient = _containerClient.GetBlobClient(fileName);

        if (content.CanSeek) content.Position = 0;

        var headers = new BlobHttpHeaders { ContentType = ContentTypeHelper.FromExtension(extension) };

        await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);

        return blobClient.Uri.ToString();
    }
}