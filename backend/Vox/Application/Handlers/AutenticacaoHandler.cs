using Vox.Application.Services;
using Vox.Domain.Enums;
using Vox.Infrastructure.Repositories;

namespace Vox.Application.Handlers;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

using Vox.Domain.Models;

public class AutenticacaoHandler
{
    private readonly string _jwtSecret;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IMedicoRepository _medicoRepository;

    public AutenticacaoHandler(
        IConfiguration configuration,
        IPacienteRepository pacienteRepository,
        IMedicoRepository medicoRepository)
    {
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
                     ?? configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(_jwtSecret) || _jwtSecret.Length < 32)
            throw new Exception("JWT_SECRET inválido ou muito curto. Deve ter no mínimo 32 bytes.");

        _pacienteRepository = pacienteRepository;
        _medicoRepository = medicoRepository;
    }

    public string CriarHash(string senha, out string saltBase64)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        saltBase64 = Convert.ToBase64String(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(senha, salt, 100_000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }

    public string CompararHash(string senha, string saltBase64)
    {
        var salt = Convert.FromBase64String(saltBase64);

        using var pbkdf2 = new Rfc2898DeriveBytes(senha, salt, 100_000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }

    public async Task<string> GerarTokenJWT(UsuarioModel usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        string? id = null;

        if (usuario.Tipo == TipoUsuarioEnum.Paciente)
        {
            var paciente = await _pacienteRepository.BuscarPorUsuarioId(usuario.Id);
            id = paciente?.Id.ToString();
        }
        else if (usuario.Tipo == TipoUsuarioEnum.Medico)
        {
            var medico = await _medicoRepository.BuscarPorUsuarioId(usuario.Id);
            id = medico?.Id.ToString();
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", id),
                new Claim("usuarioId", usuario.Id.ToString()),
                new Claim("tipo", usuario.Tipo.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = "Vox.API",
            Audience = "Vox.Client",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public IDictionary<string, string> LerDadosToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        var jwtToken = handler.ReadJwtToken(token);
        var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);

        return claims;
    }
}
