using Microsoft.IdentityModel.Tokens;
using Vox.Application.Handlers;
using Vox.Infrastructure.Utils;

namespace Vox.Application.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Vox.Application.DTOs.Paciente;
using Vox.Infrastructure.Repositories;
using Vox.Domain.Factories;
using Vox.Domain.Models;

public interface IPacienteService
{
    Task<PacienteModel?> BuscarPorId(int id, string? token);
    Task<PacienteModel?> BuscarPorUsuarioId(int usuarioId);
    Task<PacienteModel> Adicionar(CadastroPacienteDTO dto);
    Task<PacienteModel?> Atualizar(AtualizarPacienteDTO dto, string token);
}

public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _repository;
    private readonly PacienteFactory _factory;
    private readonly IUsuarioService _usuarioService;
    private readonly PacienteHandler _handler;
    private readonly CacheManager _cacheManager;

    public PacienteService(
        IPacienteRepository repository,
        PacienteFactory factory,
        IUsuarioService usuarioService,
        PacienteHandler pacienteHandler,
        CacheManager cacheManager
    )
    {
        _repository = repository;
        _factory = factory;
        _usuarioService = usuarioService;
        _handler = pacienteHandler;
        _cacheManager = cacheManager;
    }
    
    public async Task<PacienteModel?> BuscarPorId(int id, string? token)
    {
        if (token == null)
            throw new SecurityTokenInvalidTypeException("Você precisa estar autenticado para usar este recurso.");

        var handlerJwt = new JwtSecurityTokenHandler();
        var jwtToken = handlerJwt.ReadJwtToken(token);
        var tipoUsuario = jwtToken.Claims.FirstOrDefault(c => c.Type == "tipo")?.Value;

        if (tipoUsuario == "Medico")
            // ...
            throw new UnauthorizedAccessException("Para ter acesso aos dados de um paciente, o medico precisa ter uma consulta com ele.");

        var cacheKey = $"Paciente:{id}";
        var cached = await _cacheManager.GetAsync<PacienteModel>(cacheKey);
        if (cached != null)
            return cached;

        var paciente = await _repository.BuscarPorId(id);
        if (paciente != null)
            await _cacheManager.SetAsync(cacheKey, paciente, TimeSpan.FromMinutes(5));

        return paciente;
    }
    
    public async Task<PacienteModel?> BuscarPorUsuarioId(int usuarioId)
    {
        var cacheKey = $"PacienteUsuario:{usuarioId}";
        var cached = await _cacheManager.GetAsync<PacienteModel>(cacheKey);
        if (cached != null)
            return cached;

        var paciente = await _repository.BuscarPorUsuarioId(usuarioId);
        if (paciente != null)
            await _cacheManager.SetAsync(cacheKey, paciente, TimeSpan.FromMinutes(5));

        return paciente;
    }

    public async Task<PacienteModel> Adicionar(CadastroPacienteDTO dto)
    {
        var usuario = await _usuarioService.Adicionar(dto.Usuario);
        var paciente = _factory.Criar(dto, usuario);
        paciente = await _repository.Adicionar(paciente);

        await _cacheManager.RemoveByPrefixAsync("Paciente");

        return paciente;
    }
    
    public async Task<PacienteModel?> Atualizar(AtualizarPacienteDTO dto, string token)
    {
        var paciente = await _handler.GetPacienteFromToken(token);
        if (paciente == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Nome)) paciente.Nome = dto.Nome;
        if (dto.DataNascimento.HasValue) paciente.DataNascimento = dto.DataNascimento.Value;
        if (!string.IsNullOrWhiteSpace(dto.Telefone)) paciente.Telefone = dto.Telefone;
        if (!string.IsNullOrWhiteSpace(dto.Email)) paciente.Email = dto.Email;

        paciente = await _repository.Atualizar(paciente);

        await _cacheManager.RemoveAsync($"Paciente:{paciente.Id}");
        await _cacheManager.RemoveAsync($"PacienteUsuario:{paciente.UsuarioId}");
        await _cacheManager.RemoveByPrefixAsync("Paciente");

        return paciente;
    }
}
