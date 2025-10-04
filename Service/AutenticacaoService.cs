using System.Security.Cryptography;
using System.Text;
using Dominio;
using Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Dtos;
using Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Service;

public class AutenticacaoService(
    IUserRepository userRepository,
    IConfiguration config) : IAutenticacaoService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public async Task<RespostaAutenticacaoDto> RegisterAsync(RegistrarUsuarioDto dto)
    {
        var existing = await userRepository.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());
        if (existing != null)
            throw new InvalidOperationException("Email já registrado.");

        var user = User.Create(dto.Nome, dto.Email, dto.Telefone);
        var hash = HashPassword(dto.Senha);
        user.SetPasswordHash(hash);
        user = await userRepository.AddAsync(user);
        return GerarResposta(user);
    }

    public async Task<RespostaAutenticacaoDto> LoginAsync(LoginDto dto)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());
        if (user == null || !VerificarSenha(dto.Senha, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas");
        return GerarResposta(user);
    }

    private RespostaAutenticacaoDto GerarResposta(User user)
    {
        var (token, expires) = GerarToken(user);
        return new RespostaAutenticacaoDto
        {
            Token = token,
            ExpiresAtUtc = expires,
            User = new UsuarioDto
            {
                Id = user.Id,
                Nome = user.Nome,
                Email = user.Email,
                Telefone = user.Telefone
            }
        };
    }

    private (string token, DateTime expires) GerarToken(User user)
    {
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");
        var issuer = config["Jwt:Issuer"] ?? "MeuCorretorApi";
        var audience = config["Jwt:Audience"] ?? issuer;
        var expireMinutes = int.TryParse(config["Jwt:ExpireMinutes"], out var m) ? m : 60;
        var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Nome)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }

    private static string HashPassword(string senha)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(senha),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
        return $"v1|{Iterations}|{Convert.ToBase64String(salt)}|{Convert.ToBase64String(hash)}";
    }

    private static bool VerificarSenha(string senha, string hashArmazenado)
    {
        try
        {
            var partes = hashArmazenado.Split('|');
            if (partes.Length != 4 || partes[0] != "v1") return false;
            var iterations = int.Parse(partes[1]);
            var salt = Convert.FromBase64String(partes[2]);
            var hash = Convert.FromBase64String(partes[3]);
            var teste = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(senha),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                hash.Length);
            return CryptographicOperations.FixedTimeEquals(hash, teste);
        }
        catch
        {
            return false;
        }
    }
}