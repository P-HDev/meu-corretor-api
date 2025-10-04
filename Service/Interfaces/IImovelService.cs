using Service.Dtos;

namespace Service.Interfaces;

public interface IImovelService
{
    Task<IEnumerable<ImovelDto>> GetAllAsync();
    Task<ImovelDto?> GetByIdAsync(Guid id);
    Task<ImovelDto?> GetByPublicIdAsync(string publicId);
    Task<ImovelDto> CreateAsync(CriarImovelDto criarImovelDto);
    Task<ImovelDto> CreateWithUploadAsync(CriarImovelUploadDto criarImovelUploadDto);
    Task UpdateAsync(Guid id, AtualizarImovelDto atualizarImovelDto);
    Task DeleteAsync(Guid id);
}