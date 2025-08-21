using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Vox.API.Hubs;

namespace Vox.Application.Services;

using System.Threading.Tasks;
using System.Linq;
using Vox.Application.DTOs.Consulta;
using Vox.Application.Handlers;
using Vox.Infrastructure.Repositories;
using Vox.Infrastructure.Utils;
using Vox.Domain.Factories;
using Vox.Domain.Models;
using Vox.Domain.Enums;

public interface IConsultaService
{
    Task<ConsultaModel?> BuscarPorId(int id, string token);
    Task<ConsultaModel[]?> BuscarTodosPorMedico(StatusConsultaEnum? status, string token);
    Task<ConsultaModel[]?> BuscarTodosPorPaciente(StatusConsultaEnum? status, string token);
    Task<ConsultaModel?> Adicionar(CadastroConsultaDTO dto, string token);
    Task<ConsultaModel?> AtualizarStatus(int id, StatusConsultaEnum novoStatus, string token);
}

public class ConsultaService : IConsultaService
{
    private readonly IConsultaRepository _repository;
    private readonly ConsultaFactory _factory;
    private readonly AutenticacaoHandler _autenticacaoHandler;
    private readonly PacienteHandler _pacienteHandler;
    private readonly MedicoHandler _medicoHandler;
    private readonly HorarioHandler _horarioHandler;
    private readonly IHorarioRepository _horarioRepository;
    private readonly INotificacaoService _notificacaoService;
    private readonly CacheManager _cacheManager;
    private readonly IHubContext<HorarioHub> _horarioHubContext;

    public ConsultaService(
        IConsultaRepository repository,
        ConsultaFactory factory,
        AutenticacaoHandler autenticacaoHandler,
        PacienteHandler pacienteHandler,
        MedicoHandler medicoHandler,
        HorarioHandler horarioHandler,
        IHorarioRepository horarioRepository,
        INotificacaoService notificacaoService,
        CacheManager cacheManager,
        IHubContext<HorarioHub> horarioHubContext
    )
    {
        _repository = repository;
        _factory = factory;
        _autenticacaoHandler = autenticacaoHandler;
        _pacienteHandler = pacienteHandler;
        _medicoHandler = medicoHandler;
        _horarioHandler = horarioHandler;
        _horarioRepository = horarioRepository;
        _notificacaoService = notificacaoService;
        _cacheManager = cacheManager;
        _horarioHubContext = horarioHubContext;
    }

    public async Task<ConsultaModel?> BuscarPorId(int id, string token)
    {
        var paciente = await _pacienteHandler.GetPacienteFromToken(token);
        if (paciente == null)
            throw new SecurityTokenException("Token informado para essa consulta e invalido.");

        var cacheKey = $"Consulta:{id}";
        var cached = await _cacheManager.GetAsync<ConsultaModel>(cacheKey);
        if (cached != null)
            return cached;

        var consulta = await _repository.BuscarPorId(id);
        if (consulta == null || consulta.Paciente.Id != paciente.Id)
            return null;

        await _cacheManager.SetAsync(cacheKey, consulta, TimeSpan.FromMinutes(5));
        return consulta;
    }

    public async Task<ConsultaModel[]?> BuscarTodosPorPaciente(StatusConsultaEnum? status, string token)
    {
        var paciente = await _pacienteHandler.GetPacienteFromToken(token);
        if (paciente == null)
            return null;
        
        var cacheKey = $"ConsultasPorPaciente:{paciente.Id}:{status}";
        var cached = await _cacheManager.GetAsync<ConsultaModel[]>(cacheKey);
        if (cached != null)
            return cached;

        var consultas = await _repository.BuscarPorPaciente(paciente.Id, status);
        await _cacheManager.SetAsync(cacheKey, consultas, TimeSpan.FromMinutes(10));

        return consultas;
    }

    public async Task<ConsultaModel[]?> BuscarTodosPorMedico(StatusConsultaEnum? status, string token)
    {
        var medico = await _medicoHandler.GetMedicoFromToken(token);
        if (medico == null)
            return null;
        
        var cacheKey = $"ConsultasPorMedico:{medico.Id}:{status}";
        var cached = await _cacheManager.GetAsync<ConsultaModel[]>(cacheKey);
        if (cached != null)
            return cached;

        return await _repository.BuscarPorMedico(medico.Id, status);
    }

    public async Task<ConsultaModel> Adicionar(CadastroConsultaDTO dto, string token)
    {
        var paciente = await _pacienteHandler.GetPacienteFromToken(token);
        if (paciente == null)
            throw new Exception("Paciente inválido."); 

        var novoHorario = await _horarioRepository.BuscarPorId(dto.HorarioId);

        var consultasExistentes = await _repository.BuscarPorPaciente(paciente.Id);
        var consultasAgendadas = consultasExistentes
            .Where(c => c.Status == StatusConsultaEnum.Agendada)
            .ToArray();
        var horariosExistentes = consultasAgendadas.Select(c => c.Horario).ToArray();
        
        bool conflito = await _horarioHandler.HorarioConflita(novoHorario, horariosExistentes);
        if (conflito)
            throw new InvalidOperationException("Você já tem uma consulta marcada para este horário.");

        var consulta = _factory.Criar(dto, paciente.Id);
        consulta = await _repository.Adicionar(consulta);
        
        await _horarioHubContext.Clients.User(consulta.Horario.MedicoId.ToString()).SendAsync("ConsultaCriada", consulta);
        
        await _cacheManager.RemoveAsync($"HorariosDisponiveis:{consulta.Horario.Medico.Id}");
        await _cacheManager.RemoveAsync($"ConsultasPorPaciente:{paciente.Id}:{StatusConsultaEnum.Agendada}");
        await _cacheManager.RemoveAsync($"ConsultasPorMedico:{consulta.Horario.Medico.Id}:{StatusConsultaEnum.Agendada}");

        return consulta;
    }

    public async Task<ConsultaModel?> AtualizarStatus(int id, StatusConsultaEnum novoStatus, string token)
    {
        var consulta = await _repository.BuscarPorId(id);
        var statusAntigo = consulta.Status;
    
        if (consulta == null) return null;

        if (consulta.Status != StatusConsultaEnum.Agendada)
            throw new Exception("Apenas é possivel alterar consultas que estão agendadas.");
    
        await _notificacaoService.CriarNotificacao(
            $"A sua consulta de {consulta.Horario.Data:dd/MM/yyyy} às {consulta.Horario.HoraInicio} foi {novoStatus}.",
            consulta
        );

        await _repository.AtualizarStatus(id, novoStatus);

        await _cacheManager.RemoveAsync($"Consulta:{consulta.Id}");
        await _cacheManager.RemoveAsync($"ConsultasPorPaciente:{consulta.PacienteId}:{statusAntigo}");
        await _cacheManager.RemoveAsync($"ConsultasPorPaciente:{consulta.PacienteId}:{novoStatus}");
        await _cacheManager.RemoveAsync($"HorariosDisponiveis:{consulta.Horario.MedicoId}");
        await _cacheManager.RemoveAsync($"ConsultasPorMedico:{consulta.Horario.MedicoId}:{statusAntigo}");
        await _cacheManager.RemoveAsync($"ConsultasPorMedico:{consulta.Horario.MedicoId}:{novoStatus}");
    
        if (novoStatus == StatusConsultaEnum.Cancelada)
        {
            var tokenObj = _autenticacaoHandler.LerDadosToken(token);
            if (tokenObj["tipo"] != TipoUsuarioEnum.Paciente.ToString()) 
                await _horarioHubContext.Clients.User(consulta.PacienteId.ToString()).SendAsync("ConsultaCancelada", consulta);
            if (tokenObj["tipo"] != TipoUsuarioEnum.Medico.ToString()) 
                await _horarioHubContext.Clients.User(consulta.Horario.MedicoId.ToString()).SendAsync("ConsultaCancelada", consulta);
        }
    
        return consulta;
    }
}
