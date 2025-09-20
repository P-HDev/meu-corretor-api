using Dominio;
using Dominio.Builders;
using Service.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace Service.Mappings
{
    public static class ImovelMappings
    {
        public static ImovelDto ToDto(this Imovel entity) => new()
        {
            Id = entity.Id,
            PublicId = entity.PublicId,
            Titulo = entity.Titulo,
            Endereco = entity.Endereco,
            Descricao = entity.Descricao,
            Status = entity.Status,
            Preco = entity.Preco,
            Area = entity.Area,
            Quartos = entity.Quartos,
            Banheiros = entity.Banheiros,
            Suites = entity.Suites,
            Vagas = entity.Vagas,
            CorretorTelefone = entity.CorretorTelefone,
            UserId = entity.UserId,
            Imagens = entity.Imagens.Select(i => new ImagemDto { Id = i.Id, Url = i.Url }).ToList()
        };

        public static IEnumerable<ImovelDto> ToDtoList(this IEnumerable<Imovel> entities) => entities.Select(e => e.ToDto());

        public static Imovel ToEntity(this CreateImovelDto dto)
        {
            var builder = ImovelBuilder.Novo()
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
                // telefone será injetado no service, então builder será completado lá
                .AdicionarImagens(dto.Imagens.Select(i => i.Url));
            return builder.Build();
        }

        public static void ApplyUpdate(this Imovel entity, UpdateImovelDto dto)
        {
            entity.Atualizar(dto.Titulo, dto.Endereco, dto.Descricao, dto.Status, dto.Preco,
                dto.Area, dto.Quartos, dto.Banheiros, dto.Suites, dto.Vagas);
            entity.SubstituirImagens(dto.Imagens.Select(i => i.Url));
        }
    }
}