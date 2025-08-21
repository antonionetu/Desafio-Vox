using Microsoft.EntityFrameworkCore;
using Vox.Application.Handlers;
using Vox.Infrastructure.Utils;

namespace Vox.Application.Services;

using System.Threading.Tasks;
using System.Linq;
using Vox.Application.DTOs;
using Vox.Application.DTOs.Medico;
using Vox.Infrastructure.Repositories;
using Vox.Domain.Factories;
using Vox.Domain.Models;

public interface IMedicoService
{
    Task<MedicosComEspecialidadesDTO> BuscarTodos(int limit, string? nome, string? especialidade);
    Task<MedicoModel?> BuscarPorId(int id);
    Task<MedicoModel?> BuscarPorUsuarioId(int usuarioId);
    Task<MedicoModel> Adicionar(CadastroMedicoDTO dto);
    Task<MedicoModel?> Atualizar(AtualizaMedicoDTO dto, string token);
}

public class MedicoService : IMedicoService
{
    private readonly IMedicoRepository _repository;
    private readonly MedicoFactory _factory;
    private readonly IUsuarioService _usuarioService;
    private readonly MedicoHandler _handler;
    private readonly CacheManager _cacheManager;

    public MedicoService(
        IMedicoRepository repository,
        MedicoFactory factory,
        IUsuarioService usuarioService,
        MedicoHandler handler,
        CacheManager cacheManager
    )
    {
        _repository = repository;
        _factory = factory;
        _usuarioService = usuarioService;
        _handler = handler;
        _cacheManager = cacheManager;
    }
    
    public async Task<MedicosComEspecialidadesDTO> BuscarTodos(int limit = 10, string? nome = null, string? especialidade = null)
    {
        var query = _repository.BuscarTodos().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var normalizedNome = nome.ToLower();
            query = query.Where(m => m.Nome.ToLower().Contains(normalizedNome));
        }

        List<string> especialidadesList = new();
        if (!string.IsNullOrWhiteSpace(especialidade))
        {
            especialidadesList = especialidade
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().ToLower())
                .ToList();

            query = query.Where(m => especialidadesList.Any(e => m.Especialidade.ToLower().Contains(e)));
        }

        var medicosFiltrados = await query
            .OrderBy(m => m.Nome)
            .Take(limit)
            .Select(m => MedicoOutputDTO.FromModel(m))
            .ToListAsync();

        var todasEspecialidades = await _repository.BuscarTodos()
            .Select(m => m.Especialidade)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();

        return new MedicosComEspecialidadesDTO
        {
            Medicos = medicosFiltrados,
            Especialidades = todasEspecialidades
        };
    }


    public async Task<MedicoModel?> BuscarPorId(int id)
    {
        var cacheKey = $"Medico:{id}";
        var cached = await _cacheManager.GetAsync<MedicoModel>(cacheKey);
        if (cached != null)
            return cached;

        var medico = await _repository.BuscarPorId(id);
        if (medico != null)
            await _cacheManager.SetAsync(cacheKey, medico, TimeSpan.FromMinutes(5));

        return medico;
    }

    public async Task<MedicoModel?> BuscarPorUsuarioId(int usuarioId)
    {
        var cacheKey = $"MedicoUsuario:{usuarioId}";
        var cached = await _cacheManager.GetAsync<MedicoModel>(cacheKey);
        if (cached != null)
            return cached;

        var medico = await _repository.BuscarPorUsuarioId(usuarioId);
        if (medico != null)
            await _cacheManager.SetAsync(cacheKey, medico, TimeSpan.FromMinutes(5));

        return medico;
    }
    
    public async Task<MedicoModel> Adicionar(CadastroMedicoDTO dto)
    {
        var usuario = await _usuarioService.Adicionar(dto.Usuario);
        var medico = _factory.Criar(dto, usuario);
        medico = await _repository.Adicionar(medico);

        await _cacheManager.RemoveByPrefixAsync("Medicos:");

        return medico;
    }
    
    public async Task<MedicoModel?> Atualizar(AtualizaMedicoDTO dto, string token)
    {
        var medico = await _handler.GetMedicoFromToken(token);
        if (medico == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Nome)) medico.Nome = dto.Nome;
        if (!string.IsNullOrWhiteSpace(dto.Especialidade)) medico.Especialidade = dto.Especialidade;
        if (dto.Sexo.HasValue) medico.Sexo = dto.Sexo.Value;
        if (dto.DataNascimento.HasValue) medico.DataNascimento = dto.DataNascimento.Value;
        if (!string.IsNullOrWhiteSpace(dto.Telefone)) medico.Telefone = dto.Telefone;
        if (!string.IsNullOrWhiteSpace(dto.Email)) medico.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.CRM)) medico.CRM = dto.CRM;
        if (!string.IsNullOrWhiteSpace(dto.Descricao)) medico.Descricao = dto.Descricao;

        await _repository.Atualizar(medico);

        await _cacheManager.RemoveAsync($"Medico:{medico.Id}");
        await _cacheManager.RemoveAsync($"MedicoUsuario:{medico.UsuarioId}");
        await _cacheManager.RemoveByPrefixAsync("Medicos:");

        return medico;
    }
}
