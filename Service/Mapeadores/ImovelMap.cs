using Dominio;
using Dominio.Builders;
using Service.Dtos;

namespace Service.Mapeadores;

public static class ImovelMap
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
        ImagensUrls = entity.ImagensUrls.ToList()
    };


    public static Imovel ToEntity(this CriarImovelDto dto)
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
            .ComVagas(dto.Vagas);
        return builder.Build();
    }

    public static void ApplyUpdate(this Imovel entity, AtualizarImovelDto dto)
    {
        entity.Atualizar(dto.Titulo, dto.Endereco, dto.Descricao, dto.Status, dto.Preco,
            dto.Area, dto.Quartos, dto.Banheiros, dto.Suites, dto.Vagas);

        entity.DefinirImagens(dto.ImagensUrls);
    }
}