namespace Vox.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

using Vox.Infrastructure.Data;
using Vox.Domain.Enums;
using Vox.Domain.Models;

public interface IConsultaRepository
{
    Task<ConsultaModel?> BuscarPorId(int id);
    Task<ConsultaModel[]> BuscarPorPaciente(int pacienteId, StatusConsultaEnum? status = null);
    Task<ConsultaModel[]> BuscarPorMedico(int medicoId, StatusConsultaEnum? status = null);
    Task<ConsultaModel[]> BuscarPorHorarioId(int horarioId, StatusConsultaEnum? status);
    Task<ConsultaModel> Adicionar(ConsultaModel consulta);
    Task AtualizarStatus(int consultaId, StatusConsultaEnum novoStatus);
}

public class ConsultaRepository(AppDbContext DbContext) : IConsultaRepository
{
    public async Task<ConsultaModel?> BuscarPorId(int id)
    {
        return await DbContext.Consultas
            .Include(c => c.Horario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ConsultaModel[]> BuscarPorHorarioId(int horarioId, StatusConsultaEnum? status = null)
    {
        var query = DbContext.Consultas
            .Where(c => c.HorarioId == horarioId)
            .Include(c => c.Horario)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await query.ToArrayAsync();
    }

    public async Task<ConsultaModel[]> BuscarPorPaciente(int pacienteId, StatusConsultaEnum? status = null)
    {
        var query = DbContext.Consultas
            .Where(c => c.PacienteId == pacienteId)
            .Include(c => c.Horario)
                .ThenInclude(h => h.Medico)
            .AsQueryable();

        if (status != null)
            query = query.Where(c => c.Status == status.Value);

        return await query.ToArrayAsync();
    }

    public async Task<ConsultaModel[]> BuscarPorMedico(int medicoId, StatusConsultaEnum? status = null)
    {
        var query = DbContext.Consultas
            .Include(c => c.Horario)
            .Include(c => c.Paciente)
            .Where(c => c.Horario.Medico.Id == medicoId)
            .AsQueryable();

        if (status != null)
            query = query.Where(c => c.Status == status.Value);

        return await query.ToArrayAsync();
    }

    public async Task<ConsultaModel?> Adicionar(ConsultaModel consulta)
    {
        await DbContext.Consultas.AddAsync(consulta);
        await DbContext.SaveChangesAsync();

        return await DbContext.Consultas
            .Include(c => c.Horario)
            .FirstOrDefaultAsync(c => c.Id == consulta.Id);
    }

    public async Task AtualizarStatus(int consultaId, StatusConsultaEnum novoStatus)
    {
        var consulta = await DbContext.Consultas
            .Include(c => c.Horario)
            .FirstOrDefaultAsync(c => c.Id == consultaId);
    
        if (consulta == null)
            return;

        consulta.Status = novoStatus;
        DbContext.Consultas.Update(consulta);
    
        await DbContext.SaveChangesAsync();
    }
}