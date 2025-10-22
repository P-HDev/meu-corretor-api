using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dominio.Interfaces;

namespace InfraEstrutura.Storage;

public class AzureBlobImageStorage : IImageStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobImageStorage(BlobServiceClient blobServiceClient, string containerName = "imagens")
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
    }

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extension)) 
            extension = ".jpg";
        
        if (!extension.StartsWith('.')) 
            extension = "." + extension;

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        
        if (!(await containerClient.ExistsAsync(cancellationToken)))
        {
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var blobClient = containerClient.GetBlobClient(fileName);
        
        if (content.CanSeek)
            content.Position = 0;

        var blobHttpHeaders = new BlobHttpHeaders { ContentType = InfraEstrutura.Storage.ContentTypeHelper.FromExtension(extension) };

        await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = blobHttpHeaders }, cancellationToken);

        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new Azure.Storage.Sas.BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };
            sasBuilder.SetPermissions(Azure.Storage.Sas.BlobSasPermissions.Read);
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        return blobClient.Uri.ToString();
    }
}