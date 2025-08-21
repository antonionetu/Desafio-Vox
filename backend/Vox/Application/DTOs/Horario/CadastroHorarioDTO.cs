
using Vox.Application.DTOs.Medico;

namespace Vox.Application.DTOs.Horario;

using System;
using System.ComponentModel.DataAnnotations;

using Vox.Domain.Models;

public class CadastroHorarioDTO
{
    [Required(ErrorMessage = "A data do horário é obrigatória.")]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; }

    [Required(ErrorMessage = "O horário de início é obrigatório.")]
    [DataType(DataType.Time)]
    public TimeSpan HoraInicio { get; set; }

    [Required(ErrorMessage = "O horário de fim é obrigatório.")]
    [DataType(DataType.Time)]
    public TimeSpan HoraFim { get; set; }
}

public class HorarioOutputDTO
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
    public MedicoOutputDTO Medico { get; set; }

    public static HorarioOutputDTO FromModel(HorarioModel horario)
    {
        return new HorarioOutputDTO
        {
            Id = horario.Id,
            Data = horario.Data,
            HoraInicio = horario.HoraInicio,
            HoraFim = horario.HoraFim,
            Medico = MedicoOutputDTO.FromModel(horario.Medico)
        };
    }
}