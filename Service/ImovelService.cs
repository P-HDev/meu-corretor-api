using Dominio;
using Dominio.Interfaces;
using Service.Interfaces;
using Service.Dtos;
using Dominio.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Service.Mapeadores;
using InfraEstrutura.ContextoBancoPsql;
using Microsoft.EntityFrameworkCore;

namespace Service;

public class ImovelService(
    ContextoDb context,
    IImageStorage imageStorage,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor) : IImovelService
{
    private readonly string _publicBaseUrl = configuration["PublicBaseUrl"]?.TrimEnd('/') ?? string.Empty;

    public async Task<IEnumerable<ImovelDto>> GetAllAsync()
    {
        var usuario = await ObterUsuarioAutenticadoAsync();
        if (usuario == null) return Enumerable.Empty<ImovelDto>();

        var imoveis = await BuscarImoveisPorUsuarioAsync(usuario.Id);
        return imoveis.Select(imovel => EnriquecerDto(imovel.ToDto()));
    }

    public async Task<ImovelDto?> GetByIdAsync(Guid id)
    {
        var imovel = await BuscarImovelPorIdAsync(id);
        return imovel?.ToDto() is { } dto ? EnriquecerDto(dto) : null;
    }

    public async Task<ImovelDto?> GetByPublicIdAsync(string publicId)
    {
        var imovel = await BuscarImovelPorPublicIdAsync(publicId);
        return imovel?.ToDto() is { } dto ? EnriquecerDto(dto) : null;
    }

    public async Task<ImovelDto> CreateAsync(CriarImovelDto dto)
    {
        var usuario = await ObterUsuarioAutenticadoAsync();
        var imovel = CriarImovelAPartirDto(dto, usuario);
        
        await SalvarImovelAsync(imovel);
        return EnriquecerDto(imovel.ToDto());
    }

    public async Task<ImovelDto> CreateWithUploadAsync(CriarImovelUploadDto dto)
    {
        var usuario = await ObterUsuarioAutenticadoAsync();
        var imovel = await CriarImovelComImagensAsync(dto, usuario);
        
        await SalvarImovelAsync(imovel);
        return EnriquecerDto(imovel.ToDto());
    }

    public async Task UpdateAsync(Guid id, AtualizarImovelDto dto)
    {
        var imovel = await BuscarImovelPorIdAsync(id);
        if (imovel == null) throw new KeyNotFoundException("Imóvel não encontrado");
        
        AtualizarImovelComDto(imovel, dto);
        await SalvarAlteracoesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var imovel = await BuscarImovelPorIdAsync(id);
        if (imovel != null)
        {
            RemoverImovel(imovel);
            await SalvarAlteracoesAsync();
        }
    }

    private async Task<User?> ObterUsuarioAutenticadoAsync()
    {
        var email = ExtrairEmailDoContexto();
        return email != null ? await BuscarUsuarioPorEmailAsync(email) : null;
    }

    private string? ExtrairEmailDoContexto()
    {
        var httpUser = httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated != true) return null;
        
        var email = httpUser.FindFirstValue(ClaimTypes.Email) ?? httpUser.FindFirstValue("email");
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }

    private async Task<User?> BuscarUsuarioPorEmailAsync(string email) =>
        await context.Users.FirstOrDefaultAsync(u => u.Email == email);

    private async Task<List<Imovel>> BuscarImoveisPorUsuarioAsync(Guid userId) =>
        await context.Imoveis
            .Where(i => i.UserId == userId)
            .ToListAsync();

    private async Task<Imovel?> BuscarImovelPorIdAsync(Guid id) =>
        await context.Imoveis.FirstOrDefaultAsync(i => i.Id == id);

    private async Task<Imovel?> BuscarImovelPorPublicIdAsync(string publicId) =>
        await context.Imoveis.FirstOrDefaultAsync(i => i.PublicId == publicId);

    private static Imovel CriarImovelAPartirDto(CriarImovelDto dto, User? usuario)
    {
        var imovel = dto.ToEntity();
        if (usuario != null)
        {
            imovel.DefinirUserId(usuario.Id);
            imovel.DefinirCorretorTelefone(usuario.Telefone);
        }
        return imovel;
    }

    private async Task<Imovel> CriarImovelComImagensAsync(CriarImovelUploadDto dto, User? usuario)
    {
        var builder = ConstruirImovelBuilder(dto, usuario?.Telefone ?? string.Empty);
        await AdicionarImagensAoBuilderAsync(builder, dto.Imagens);
        
        var imovel = builder.Build();
        if (usuario != null) imovel.DefinirUserId(usuario.Id);
        
        return imovel;
    }

    private static ImovelBuilder ConstruirImovelBuilder(CriarImovelUploadDto dto, string telefone) =>
        ImovelBuilder.Novo()
            .ComTitulo(dto.Titulo)
            .ComEndereco(dto.Endereco)
            .ComDescricao(dto.Descricao)
            .ComStatus(dto.Status)
            .ComPreco(dto.Preco)
            .ComArea(dto.Area)
            .ComQuartos(dto.Quartos)
            .ComBanheiros(dto.Banheiros)
            .ComSuites(dto.Suites)
            .ComVagas(dto.Vagas)
            .ComCorretorTelefone(telefone);

    private async Task AdicionarImagensAoBuilderAsync(ImovelBuilder builder, List<IFormFile> imagens)
    {
        var extensoesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        foreach (var file in imagens.Where(f => f.Length > 0))
        {
            var extensao = ObterExtensaoValida(file.FileName, extensoesPermitidas);
            if (extensao == null) continue;

            var url = await FazerUploadImagemAsync(file, extensao);
            builder.AdicionarImagem(url);
        }
    }

    private static string? ObterExtensaoValida(string fileName, HashSet<string> extensoesPermitidas)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
        return extensoesPermitidas.Contains(ext) ? ext : null;
    }

    private async Task<string> FazerUploadImagemAsync(IFormFile file, string extensao)
    {
        using var stream = file.OpenReadStream();
        return await imageStorage.SaveAsync(stream, extensao);
    }

    private static void AtualizarImovelComDto(Imovel imovel, AtualizarImovelDto dto) =>
        imovel.ApplyUpdate(dto);

    private async Task SalvarImovelAsync(Imovel imovel)
    {
        context.Imoveis.Add(imovel);
        await SalvarAlteracoesAsync();
    }

    private void RemoverImovel(Imovel imovel) => context.Imoveis.Remove(imovel);

    private async Task SalvarAlteracoesAsync() => await context.SaveChangesAsync();

    private ImovelDto EnriquecerDto(ImovelDto dto) =>
        dto with { ShareUrl = ConstruirShareUrl(dto.PublicId) };

    private string ConstruirShareUrl(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return string.Empty;
        var baseUrl = string.IsNullOrEmpty(_publicBaseUrl) ? string.Empty : _publicBaseUrl;
        return $"{baseUrl}/api/imoveis/public/{publicId}";
    }
}