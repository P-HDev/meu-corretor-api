namespace InfraEstrutura.Storage;

internal static class ContentTypeHelper
{
    public static string FromExtension(string extension)
    {
        extension = extension.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            _ => "image/jpeg"
        };
    }
}