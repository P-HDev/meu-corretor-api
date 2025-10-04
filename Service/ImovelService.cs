using Dominio;
using Dominio.Interfaces;
using Service.Interfaces;
using Service.Dtos;
using Dominio.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Service.Mapeadores;

namespace Service;

public class ImovelService(
    IImovelRepository imovelRepository,
    IImageStorage imageStorage,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository) : IImovelService
{
    private readonly string _publicBaseUrl = configuration["PublicBaseUrl"]?.TrimEnd('/') ?? string.Empty;
    private readonly bool _serveImagesViaController = bool.TryParse(configuration["ServeImagesViaController"], out var flag) && flag;

    private async Task<User?> ObterUsuarioAsync()
    {
        var httpUser = httpContextAccessor.HttpContext?.User;
        if (httpUser == null || !(httpUser.Identity?.IsAuthenticated ?? false)) return null;
        var email = httpUser.FindFirstValue(ClaimTypes.Email) ?? httpUser.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email)) return null;
        return await userRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());
    }

    private async Task<string> ObterTelefoneCorretorAsync()
    {
        var user = await ObterUsuarioAsync();
        return user?.Telefone ?? string.Empty;
    }

    private string ConverterUrlImagem(string original)
    {
        if (string.IsNullOrWhiteSpace(original)) return original;
        if (string.IsNullOrEmpty(_publicBaseUrl)) return original;
        if (_serveImagesViaController)
        {
            var fileName = original.TrimStart('/')
                .Replace("imagens/", string.Empty, StringComparison.OrdinalIgnoreCase);
            return _publicBaseUrl + "/api/imagens/" + fileName;
        }

        if (!original.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return _publicBaseUrl + original;
        return original;
    }

    private string BuildShareUrl(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return string.Empty;
        if (string.IsNullOrEmpty(_publicBaseUrl)) return $"/api/imoveis/public/{publicId}";
        return $"{_publicBaseUrl}/api/imoveis/public/{publicId}";
    }

    private ImovelDto EnriquecerDto(ImovelDto dto)
    {
        var imagensEnriquecidas = dto.Imagens.Select(img => img with { Url = ConverterUrlImagem(img.Url) }).ToList();
        return dto with
        {
            Imagens = imagensEnriquecidas,
            ShareUrl = BuildShareUrl(dto.PublicId)
        };
    }

    private IEnumerable<ImovelDto> EnriquecerDtos(IEnumerable<ImovelDto> dtos)
    {
        return dtos.Select(EnriquecerDto);
    }

    public async Task<IEnumerable<ImovelDto>> GetAllAsync()
    {
        var usuario = await ObterUsuarioAsync();
        if (usuario == null)
        {
            return Enumerable.Empty<ImovelDto>();
        }

        var imoveis = await imovelRepository.GetAllByUserAsync(usuario.Id);
        var lista = imoveis.ToDtoList();
        return EnriquecerDtos(lista);
    }

    public async Task<ImovelDto?> GetByIdAsync(Guid id)
    {
        var imovel = await imovelRepository.GetByIdAsync(id);
        var dto = imovel?.ToDto();
        return dto != null ? EnriquecerDto(dto) : null;
    }

    public async Task<ImovelDto?> GetByPublicIdAsync(string publicId)
    {
        var imovel = await imovelRepository.GetByPublicIdAsync(publicId);
        var dto = imovel?.ToDto();
        return dto != null ? EnriquecerDto(dto) : null;
    }

    public async Task<ImovelDto> CreateAsync(CriarImovelDto criarImovelDto)
    {
        var usuario = await ObterUsuarioAsync();
        var imovel = criarImovelDto.ToEntity();
        if (usuario != null)
        {
            imovel.DefinirOwner(usuario.Id);
            imovel.DefinirCorretorTelefone(usuario.Telefone);
        }
        else
        {
            var tel = await ObterTelefoneCorretorAsync();
            imovel.DefinirCorretorTelefone(tel);
        }

        var createdImovel = await imovelRepository.AddAsync(imovel);
        var dto = createdImovel.ToDto();
        return EnriquecerDto(dto);
    }

    public async Task<ImovelDto> CreateWithUploadAsync(CriarImovelUploadDto criarImovelUploadDto)
    {
        var usuario = await ObterUsuarioAsync();
        var telefoneCorretor = usuario?.Telefone ?? string.Empty;
        var builder = ImovelBuilder.Novo()
            .ComTitulo(criarImovelUploadDto.Titulo)
            .ComEndereco(criarImovelUploadDto.Endereco)
            .ComDescricao(criarImovelUploadDto.Descricao)
            .ComStatus(criarImovelUploadDto.Status)
            .ComPreco(criarImovelUploadDto.Preco)
            .ComArea(criarImovelUploadDto.Area)
            .ComQuartos(criarImovelUploadDto.Quartos)
            .ComBanheiros(criarImovelUploadDto.Banheiros)
            .ComSuites(criarImovelUploadDto.Suites)
            .ComVagas(criarImovelUploadDto.Vagas)
            .ComCorretorTelefone(telefoneCorretor);

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (criarImovelUploadDto.Imagens != null)
        {
            foreach (var file in criarImovelUploadDto.Imagens)
            {
                if (file is { Length: > 0 })
                {
                    var ext = System.IO.Path.GetExtension(file.FileName);
                    if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
                    if (!allowed.Contains(ext)) continue;
                    using var stream = file.OpenReadStream();
                    var url = await imageStorage.SaveAsync(stream, ext);
                    builder.AdicionarImagem(url);
                }
            }
        }

        var imovel = builder.Build();
        if (usuario != null)
        {
            imovel.DefinirOwner(usuario.Id);
        }

        var created = await imovelRepository.AddAsync(imovel);
        var dto = created.ToDto();
        return EnriquecerDto(dto);
    }

    public async Task UpdateAsync(Guid id, AtualizarImovelDto atualizarImovelDto)
    {
        var imovel = await imovelRepository.GetByIdAsync(id);
        if (imovel == null) throw new KeyNotFoundException("Imóvel não encontrado");
        imovel.ApplyUpdate(atualizarImovelDto);
        await imovelRepository.UpdateAsync(imovel);
    }

    public async Task DeleteAsync(Guid id)
    {
        await imovelRepository.DeleteAsync(id);
    }
}