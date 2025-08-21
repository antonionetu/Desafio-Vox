using Microsoft.AspNetCore.SignalR;
using Vox.API.Hubs;

namespace Vox.Application.Services;

using System.Threading.Tasks;
using System.Linq;

using Vox.Application.DTOs.Horario;
using Vox.Application.Handlers;
using Vox.Infrastructure.Repositories;
using Vox.Infrastructure.Utils;
using Vox.Domain.Factories;
using Vox.Domain.Models;
using Vox.Domain.Enums;

public interface IHorarioService
{
    Task<HorarioModel[]?> BuscaTodos(int id);
    Task<HorarioModel?> BuscarPorId(int id, string token);
    Task<HorarioModel?> Adiciona(CadastroHorarioDTO dto, string token);
    Task<HorarioModel?> Edita(int id, CadastroHorarioDTO dto, string token);
    Task<HorarioModel?> Deleta(int id, string token);
}

public class HorarioService : IHorarioService
{
    private readonly IHorarioRepository _repository;
    private readonly IConsultaRepository _consultaRepository;
    private readonly IConsultaService _consultaService;
    private readonly INotificacaoService _notificacaoService;
    private readonly HorarioFactory _factory;
    private readonly HorarioHandler _handler;
    private readonly MedicoHandler _medicoHandler;
    private readonly ConsultaHandler _consultaHandler;
    private readonly CacheManager _cacheManager;
    private readonly IHubContext<HorarioHub> _hubContext;

    public HorarioService(
        IHorarioRepository repository,
        IConsultaRepository consultaRepository,
        IConsultaService consultaService,
        INotificacaoService notificacaoService,
        HorarioFactory factory,
        HorarioHandler handler,
        MedicoHandler medicoHandler,
        ConsultaHandler consultaHandler,
        CacheManager cacheManager,
        IHubContext<HorarioHub> hubContext
    )
    {
        _repository = repository;
        _consultaRepository = consultaRepository;
        _consultaService = consultaService;
        _notificacaoService = notificacaoService;
        _factory = factory;
        _medicoHandler = medicoHandler;
        _handler = handler;
        _consultaHandler = consultaHandler;
        _cacheManager = cacheManager;
        _hubContext = hubContext;
    }
    
    public async Task<HorarioModel[]?> BuscaTodos(int medicoId)
    {
        var cacheKey = $"HorariosDisponiveis:{medicoId}";
        var cached = await _cacheManager.GetAsync<HorarioModel[]>(cacheKey);
        if (cached != null)
            return cached;

        var todosHorarios = await _repository.BuscaTodosPorMedico(medicoId);
        var consultasDoMedico = await _consultaRepository.BuscarPorMedico(medicoId);

        var horariosDisponiveis = todosHorarios
            .Where(h => !consultasDoMedico.Any(
                c => c.HorarioId == h.Id && 
                c.Status != StatusConsultaEnum.Cancelada
            ))
            .ToArray();

        await _cacheManager.SetAsync(cacheKey, horariosDisponiveis, TimeSpan.FromMinutes(5));

        return horariosDisponiveis;
    }

    public async Task<HorarioModel?> BuscarPorId(int id, string token)
    {
        var medico = await _medicoHandler.GetMedicoFromToken(token);
        if (medico == null) return null;

        var cacheKey = $"Horario:{id}";
        var cached = await _cacheManager.GetAsync<HorarioModel>(cacheKey);
        if (cached != null)
            return cached;

        var horario = await _repository.BuscarPorId(id);
        if (horario == null || horario.MedicoId != medico.Id) return null;

        await _cacheManager.SetAsync(cacheKey, horario, TimeSpan.FromMinutes(5));
        return horario;
    }

    public async Task<HorarioModel?> Adiciona(CadastroHorarioDTO dto, string token)
    {
        var medico = await _medicoHandler.GetMedicoFromToken(token);
        if (medico == null) return null;

        var horario = _factory.Criar(dto, medico);
        
        var horariosExistentes = await _repository.BuscaTodosPorMedico(medico.Id);
        bool temChoqueDeHorario = horariosExistentes.Any(h =>
            h.Data == horario.Data &&
            horario.HoraInicio < h.HoraFim &&
            horario.HoraFim > h.HoraInicio
        );

        if (temChoqueDeHorario)
            throw new Exception("Voce ja tem um horario cadastrado nesse momento");

        var consultasDoMedico = await _consultaRepository.BuscarPorMedico(medico.Id);
        var horariosOcupados = consultasDoMedico
            .Where(c => c.Status == StatusConsultaEnum.Agendada)
            .Select(c => c.Horario)
            .ToArray();

        bool conflita = await _handler.HorarioConflita(horario, horariosOcupados);

        if (conflita)
            return null;

        horario = await _repository.Adicionar(horario);

        await _cacheManager.RemoveAsync($"HorariosDisponiveis:{medico.Id}");

        return horario;
    }

    public async Task<HorarioModel?> Edita(int id, CadastroHorarioDTO dto, string token)
    {
        var medico = await _medicoHandler.GetMedicoFromToken(token);
        if (medico == null) throw new Exception("Medico nao encontrado");

        var horario = await _repository.BuscarPorId(id);
        if (horario == null || horario.MedicoId != medico.Id) throw new Exception("Este horario nao pertence a esse medico");

        _factory.Atualizar(horario, dto);

        var consultasDoMedico = await _consultaRepository.BuscarPorMedico(medico.Id);
        var horariosOcupados = consultasDoMedico
            .Where(c => c.Status == StatusConsultaEnum.Agendada && c.HorarioId != horario.Id)
            .Select(c => c.Horario)
            .ToArray();

        bool conflitaHorarioMedico = await _handler.HorarioConflita(horario, horariosOcupados);
        if (conflitaHorarioMedico)
            throw new Exception("Choque de horarios.");

        var consultas = await _consultaHandler.BuscarPorHorarioId(horario.Id, StatusConsultaEnum.Agendada);
        var consulta = consultas.FirstOrDefault();

        if (consulta != null)
        {
            var consultasPaciente = await _consultaRepository.BuscarPorPaciente(consulta.Paciente.Id);
            var horariosPaciente = consultasPaciente
                .Where(c => c.HorarioId != horario.Id)
                .Select(c => c.Horario)
                .ToArray();

            bool conflitaHorarioPaciente = await _handler.HorarioConflita(horario, horariosPaciente);
            if (conflitaHorarioPaciente)
                throw new Exception("Este horário conflita com a agenda do paciente.");

            await _notificacaoService.CriarNotificacao(
                $"A sua consulta de {consulta.Horario.Data:dd/MM/yyyy} às {consulta.Horario.HoraInicio} foi reagendada.",
                consulta
            );

            string pacienteId = consulta.Paciente.Id.ToString();
            await _hubContext.Clients.User(pacienteId).SendAsync("HorarioAtualizado", horario);
        }

        await _repository.Atualizar(horario);

        await _cacheManager.RemoveAsync($"Horario:{horario.Id}");
        await _cacheManager.RemoveAsync($"HorariosDisponiveis:{medico.Id}");
        await _cacheManager.RemoveAsync($"ConsultasPorPaciente:{medico.Id}:{StatusConsultaEnum.Agendada}");
        await _cacheManager.RemoveAsync($"ConsultasPorPaciente:{medico.Id}:{StatusConsultaEnum.Finalizada}");
        
        return horario;
    }
    
    public async Task<HorarioModel?> Deleta(int id, string token)
    {
        var medico = await _medicoHandler.GetMedicoFromToken(token);
        if (medico == null) return null;

        var horario = await _repository.BuscarPorId(id);
        if (horario == null || horario.MedicoId != medico.Id)
            return null;

        var consultas = await _consultaHandler.BuscarPorHorarioId(horario.Id, StatusConsultaEnum.Agendada);

        foreach (var consulta in consultas)
        {
            await _consultaService.AtualizarStatus(consulta.Id, StatusConsultaEnum.Cancelada, token);
        }

        await _repository.Deletar(id);

        await _cacheManager.RemoveAsync($"Horario:{horario.Id}");
        await _cacheManager.RemoveAsync($"HorariosDisponiveis:{medico.Id}");

        return horario;
    }
}
