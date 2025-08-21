using Vox.Domain.Enums;

namespace Vox.Application.Handlers;

using System.Threading.Tasks;
using Vox.Domain.Models;
using Vox.Infrastructure.Repositories;
using System.Linq;

public class ConsultaHandler
{
    private readonly IConsultaRepository _consultaRepository;

    public ConsultaHandler(IConsultaRepository consultaRepository)
    {
        _consultaRepository = consultaRepository;
    }

    public async Task<ConsultaModel[]?> BuscarPorHorarioId(int horarioId, StatusConsultaEnum? status = null)
    {
        return await _consultaRepository.BuscarPorHorarioId(horarioId, status);
    }

    public async Task<ConsultaModel?> BuscarPorPacienteId(int pacienteId)
    {
        var consultas = await _consultaRepository.BuscarPorPaciente(pacienteId);
        return consultas?.FirstOrDefault();
    }
}