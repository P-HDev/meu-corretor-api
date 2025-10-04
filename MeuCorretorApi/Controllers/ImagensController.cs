using Microsoft.AspNetCore.Mvc;

namespace MeuCorretorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagensController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] MimeJpeg = { ".jpg", ".jpeg" };

    [HttpGet("{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return NotFound();
        if (fileName.Contains("..", StringComparison.Ordinal)) return NotFound();
        var webRoot = env.WebRootPath;
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

    [HttpHead("{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Head(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return NotFound();
        if (fileName.Contains("..", StringComparison.Ordinal)) return NotFound();
        var webRoot = env.WebRootPath;
        var imagensDir = Path.Combine(webRoot, "imagens");
        var fullPath = Path.Combine(imagensDir, fileName);
        return System.IO.File.Exists(fullPath) ? Ok() : NotFound();
    }
}