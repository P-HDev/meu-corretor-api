using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Dtos;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces
{
    public interface IImovelService
    {
        Task<IEnumerable<ImovelDto>> GetAllAsync();
        Task<ImovelDto?> GetByIdAsync(Guid id);
        Task<ImovelDto?> GetByPublicIdAsync(string publicId);
        Task<ImovelDto> CreateAsync(CreateImovelDto createImovelDto);
        Task<ImovelDto> CreateWithUploadAsync(CreateImovelUploadDto createImovelUploadDto);
        Task UpdateAsync(Guid id, UpdateImovelDto updateImovelDto);
        Task DeleteAsync(Guid id);
    }
}
