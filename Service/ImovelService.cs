using Dominio;
using Dominio.Interfaces;
using Service.Interfaces;
using Service.Dtos;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Service.Mappings;
using Dominio.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace Service
{
    public class ImovelService : IImovelService
    {
        private readonly IImovelRepository _imovelRepository;
        private readonly IImageStorage _imageStorage;
        private readonly string _publicBaseUrl;
        private readonly bool _serveImagesViaController;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public ImovelService(IImovelRepository imovelRepository, IImageStorage imageStorage, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _imovelRepository = imovelRepository;
            _imageStorage = imageStorage;
            _publicBaseUrl = configuration["PublicBaseUrl"]?.TrimEnd('/') ?? string.Empty;
            _serveImagesViaController = bool.TryParse(configuration["ServeImagesViaController"], out var flag) && flag;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        private async Task<User?> ObterUsuarioAsync()
        {
            var httpUser = _httpContextAccessor.HttpContext?.User;
            if (httpUser == null || !(httpUser.Identity?.IsAuthenticated ?? false)) return null;
            var email = httpUser.FindFirstValue(ClaimTypes.Email) ?? httpUser.FindFirstValue("email");
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _userRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());
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
                var fileName = original.TrimStart('/').Replace("imagens/", string.Empty, StringComparison.OrdinalIgnoreCase);
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

        private void EnriquecerDto(ImovelDto dto)
        {
            foreach (var img in dto.Imagens)
                img.Url = ConverterUrlImagem(img.Url);
            dto.ShareUrl = BuildShareUrl(dto.PublicId);
        }

        private void EnriquecerDtos(IEnumerable<ImovelDto> dtos)
        {
            foreach (var d in dtos) EnriquecerDto(d);
        }

        public async Task<IEnumerable<ImovelDto>> GetAllAsync()
        {
            // Agora lista apenas imóveis do usuário autenticado
            var usuario = await ObterUsuarioAsync();
            if (usuario == null)
            {
                return Enumerable.Empty<ImovelDto>();
            }
            var imoveis = await _imovelRepository.GetAllByUserAsync(usuario.Id);
            var lista = imoveis.ToDtoList();
            EnriquecerDtos(lista);
            return lista;
        }

        public async Task<ImovelDto?> GetByIdAsync(Guid id)
        {
            var imovel = await _imovelRepository.GetByIdAsync(id);
            var dto = imovel?.ToDto();
            if (dto != null) EnriquecerDto(dto);
            return dto;
        }

        public async Task<ImovelDto?> GetByPublicIdAsync(string publicId)
        {
            var imovel = await _imovelRepository.GetByPublicIdAsync(publicId);
            var dto = imovel?.ToDto();
            if (dto != null) EnriquecerDto(dto);
            return dto;
        }

        public async Task<ImovelDto> CreateAsync(CreateImovelDto createImovelDto)
        {
            var usuario = await ObterUsuarioAsync();
            var imovel = createImovelDto.ToEntity();
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
            var createdImovel = await _imovelRepository.AddAsync(imovel);
            var dto = createdImovel.ToDto();
            EnriquecerDto(dto);
            return dto;
        }

        public async Task<ImovelDto> CreateWithUploadAsync(CreateImovelUploadDto createImovelUploadDto)
        {
            var usuario = await ObterUsuarioAsync();
            var telefoneCorretor = usuario?.Telefone ?? string.Empty;
            var builder = ImovelBuilder.Novo()
                .ComTitulo(createImovelUploadDto.Titulo)
                .ComEndereco(createImovelUploadDto.Endereco)
                .ComDescricao(createImovelUploadDto.Descricao)
                .ComStatus(createImovelUploadDto.Status)
                .ComPreco(createImovelUploadDto.Preco)
                .ComArea(createImovelUploadDto.Area)
                .ComQuartos(createImovelUploadDto.Quartos)
                .ComBanheiros(createImovelUploadDto.Banheiros)
                .ComSuites(createImovelUploadDto.Suites)
                .ComVagas(createImovelUploadDto.Vagas)
                .ComCorretorTelefone(telefoneCorretor);

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (createImovelUploadDto.Imagens != null)
            {
                foreach (var file in createImovelUploadDto.Imagens)
                {
                    if (file is { Length: > 0 })
                    {
                        var ext = System.IO.Path.GetExtension(file.FileName);
                        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
                        if (!allowed.Contains(ext)) continue;
                        using var stream = file.OpenReadStream();
                        var url = await _imageStorage.SaveAsync(stream, ext);
                        builder.AdicionarImagem(url);
                    }
                }
            }

            var imovel = builder.Build();
            if (usuario != null)
            {
                imovel.DefinirOwner(usuario.Id);
            }
            var created = await _imovelRepository.AddAsync(imovel);
            var dto = created.ToDto();
            EnriquecerDto(dto);
            return dto;
        }

        public async Task UpdateAsync(Guid id, UpdateImovelDto updateImovelDto)
        {
            var imovel = await _imovelRepository.GetByIdAsync(id);
            if (imovel == null) throw new KeyNotFoundException("Imóvel não encontrado");
            imovel.ApplyUpdate(updateImovelDto);
            await _imovelRepository.UpdateAsync(imovel);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _imovelRepository.DeleteAsync(id);
        }
    }
}
