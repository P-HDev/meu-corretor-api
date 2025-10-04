using System.ComponentModel.DataAnnotations;

namespace Service.Dtos;

public record CriarImagemDto([Required] string Url);