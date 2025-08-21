using Vox.Application.DTOs.Medico;
using Vox.Application.DTOs.Paciente;

namespace Vox.Application.DTOs.Consulta;

using System.ComponentModel.DataAnnotations;

using Vox.Application.DTOs.Horario;
using Vox.Domain.Models;
using Vox.Domain.Enums;

public class CadastroConsultaDTO
{
    [Required(ErrorMessage = "O ID do horário é obrigatório.")]
    public int HorarioId { get; set; }
}

public class ConsultaOutputDTO
{
    public int Id { get; set; }
    public StatusConsultaEnum Status { get; set; }
    public HorarioOutputDTO Horario { get; set; } = null!;
    public PacienteOutputDTO Paciente { get; set; } = null!;

    public static ConsultaOutputDTO FromModel(ConsultaModel consulta)
    {
        return new ConsultaOutputDTO
        {
            Id = consulta.Id,
            Status = consulta.Status,
            Horario = HorarioOutputDTO.FromModel(consulta.Horario),
            Paciente = PacienteOutputDTO.FromModel(consulta.Paciente)
        };
    }
}