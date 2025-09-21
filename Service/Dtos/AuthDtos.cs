using System;
using System.ComponentModel.DataAnnotations;

namespace Service.Dtos
{
    public class RegisterUserDto
    {
        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [MaxLength(180)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Senha { get; set; } = string.Empty;
        [Required]
        [MaxLength(30)]
        public string Telefone { get; set; } = string.Empty; // formato aceito: +5511999999999 ou variações com símbolos
    }

    public class LoginUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Senha { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public UserInfoDto User { get; set; } = new();
    }

    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
    }
}
