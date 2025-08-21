namespace Vox.Domain.Models;

using System.ComponentModel.DataAnnotations;

public class HorarioModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "A data do horário é obrigatória.")]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; }

    [Required(ErrorMessage = "O horário de início é obrigatório.")]
    [DataType(DataType.Time)]
    public TimeSpan HoraInicio { get; set; }

    [Required(ErrorMessage = "O horário de fim é obrigatório.")]
    [DataType(DataType.Time)]
    public TimeSpan HoraFim { get; set; }

    [Required] public MedicoModel Medico { get; set; } = null!;
    public int MedicoId { get; set; }
}