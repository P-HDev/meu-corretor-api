using Dominio;
using Service.Interfaces;
using Service.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InfraEstrutura.ContextoBancoPsql;
using Microsoft.EntityFrameworkCore;

namespace Service;

public class AutenticacaoService(ContextoDb context, IConfiguration configuration) : IAutenticacaoService
{
    private readonly string _jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada");
    private readonly string _jwtIssuer = configuration["Jwt:Issuer"] ?? "MeuCorretorApi";
    private readonly string _jwtAudience = configuration["Jwt:Audience"] ?? "MeuCorretorApi";
    private readonly int _expireMinutes = int.TryParse(configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 60;

    public async Task<RespostaAutenticacaoDto> LoginAsync(LoginDto dto)
    {
        var usuario = await BuscarUsuarioPorEmailAsync(dto.Email);
        if (usuario == null || !VerificarSenha(dto.Senha, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Email ou senha inválidos");

        var token = GerarToken(usuario);
        return CriarRespostaAutenticacao(token, usuario);
    }

    public async Task<RespostaAutenticacaoDto> RegistrarAsync(RegistrarUsuarioDto dto)
    {
        await ValidarSeUsuarioJaExisteAsync(dto.Email);
        
        var usuario = CriarNovoUsuario(dto);
        await SalvarUsuarioAsync(usuario);
        
        var token = GerarToken(usuario);
        return CriarRespostaAutenticacao(token, usuario);
    }

    public async Task ResetarSenhaAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email inválido", nameof(email));

        var usuario = await BuscarUsuarioPorEmailAsync(email);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado");

        usuario.SetPasswordHash(CriptografarSenha("SenhaTeste123"));
        context.Users.Update(usuario);
        await context.SaveChangesAsync();
    }

    private async Task<User?> BuscarUsuarioPorEmailAsync(string email) =>
        await context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    private static bool VerificarSenha(string senha, string senhaHash) =>
        BCrypt.Net.BCrypt.Verify(senha, senhaHash);

    private async Task ValidarSeUsuarioJaExisteAsync(string email)
    {
        var usuarioExistente = await BuscarUsuarioPorEmailAsync(email);
        if (usuarioExistente != null)
            throw new InvalidOperationException("Usuário já cadastrado com este email");
    }

    private static User CriarNovoUsuario(RegistrarUsuarioDto dto) =>
        new(dto.Nome, dto.Email, CriptografarSenha(dto.Senha), dto.Telefone);

    private static string CriptografarSenha(string senha) =>
        BCrypt.Net.BCrypt.HashPassword(senha);

    private async Task SalvarUsuarioAsync(User usuario)
    {
        context.Users.Add(usuario);
        await context.SaveChangesAsync();
    }

    private string GerarToken(User usuario)
    {
        var claims = CriarClaims(usuario);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static Claim[] CriarClaims(User usuario) =>
    [
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Name, usuario.Nome),
        new Claim(ClaimTypes.Email, usuario.Email)
    ];

    private static RespostaAutenticacaoDto CriarRespostaAutenticacao(string token, User usuario) =>
        new(token, new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Telefone));
}