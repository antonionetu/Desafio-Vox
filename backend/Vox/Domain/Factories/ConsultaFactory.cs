namespace Vox.Domain.Factories;

using Vox.Application.DTOs.Consulta;
using Vox.Domain.Enums;
using Vox.Domain.Models;

public class ConsultaFactory
{
    public ConsultaModel Criar(CadastroConsultaDTO dto, int pacienteId)
    {
        return new ConsultaModel
        {
            HorarioId = dto.HorarioId,
            PacienteId = pacienteId,
            Status = StatusConsultaEnum.Agendada
        };
    }
}