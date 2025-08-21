namespace Vox.Domain.Factories;

using Vox.Application.DTOs.Horario;
using Vox.Domain.Models;

public class HorarioFactory
{
    public HorarioModel Criar(CadastroHorarioDTO dto, MedicoModel medico)
    {
        var dataUtc = DateTime.SpecifyKind(dto.Data, DateTimeKind.Utc);

        return new HorarioModel
        {
            Data = dataUtc,
            HoraInicio = dto.HoraInicio,
            HoraFim = dto.HoraFim,
            Medico = medico,
            MedicoId = medico.Id
        };
    }

    public void Atualizar(HorarioModel horario, CadastroHorarioDTO dto)
    {
        horario.Data = DateTime.SpecifyKind(dto.Data, DateTimeKind.Utc);
        horario.HoraInicio = dto.HoraInicio;
        horario.HoraFim = dto.HoraFim;
        horario.Medico = horario.Medico;
        horario.MedicoId = horario.Medico.Id;
    }
}