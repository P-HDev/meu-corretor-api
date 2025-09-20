using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dominio.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace InfraEstrutura.Storage
{
    public class LocalImageStorage : IImageStorage
    {
        private readonly IWebHostEnvironment _env;
        private const string FolderName = "imagens";

        public LocalImageStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(extension)) extension = ".jpg";
            if (!extension.StartsWith('.')) extension = "." + extension;

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var targetDir = Path.Combine(webRoot, FolderName);
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var fullPath = Path.Combine(targetDir, fileName);
            await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await content.CopyToAsync(fs, cancellationToken);
            }
            return $"/{FolderName}/{fileName}";
        }
    }
}

