namespace Vox.Domain.Models;

using System.ComponentModel.DataAnnotations;

using Vox.Domain.Enums;

public class ConsultaModel
{
    public int Id { get; set; }

    [Required]
    public StatusConsultaEnum Status { get; set; } = StatusConsultaEnum.Agendada;

    [Required]
    public HorarioModel Horario { get; set; } = null!;
    public int HorarioId { get; set; }

    [Required]
    public PacienteModel Paciente { get; set; } = null!;
    public int PacienteId { get; set; }
}
