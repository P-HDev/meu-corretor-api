using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;

namespace MeuCorretorApi.Controllers
{
    /// <summary>
    /// Fornece acesso às imagens armazenadas localmente.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ImagensController : ControllerBase
    {
        private static readonly string[] MimeJpeg = { ".jpg", ".jpeg" };
        private readonly IWebHostEnvironment _env;

        public ImagensController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Retorna a imagem original pelo nome do arquivo.
        /// </summary>
        /// <param name="fileName">Nome do arquivo (ex.: 0f3a2b...c4d8.jpg)</param>
        /// <returns>Arquivo de imagem.</returns>
        [HttpGet("{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return NotFound();
            if (fileName.Contains("..", StringComparison.Ordinal)) return NotFound(); // proteção básica
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagensDir = Path.Combine(webRoot, "imagens");
            var fullPath = Path.Combine(imagensDir, fileName);
            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                var e when Array.Exists(MimeJpeg, x => x == e) => "image/jpeg",
                _ => "application/octet-stream"
            };

            var stream = System.IO.File.OpenRead(fullPath);
            return File(stream, contentType);
        }

        /// <summary>
        /// Verifica existência da imagem sem retornar conteúdo.
        /// </summary>
        /// <param name="fileName">Nome do arquivo.</param>
        [HttpHead("{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Head(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return NotFound();
            if (fileName.Contains("..", StringComparison.Ordinal)) return NotFound();
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagensDir = Path.Combine(webRoot, "imagens");
            var fullPath = Path.Combine(imagensDir, fileName);
            return System.IO.File.Exists(fullPath) ? Ok() : NotFound();
        }
    }
}
