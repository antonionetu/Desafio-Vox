namespace Vox.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using Vox.Infrastructure.Data;
using Vox.Domain.Models;

public interface IHorarioRepository
{
    Task<HorarioModel[]> BuscaTodosPorMedico(int medicoId);
    Task<HorarioModel?> BuscarPorId(int id);
    Task<HorarioModel> Adicionar(HorarioModel horarioModel);
    Task Atualizar(HorarioModel horarioModel);
    Task Deletar(int id);
}

public class HorarioRepository(AppDbContext DbContext) : IHorarioRepository
{
    public async Task<HorarioModel[]> BuscaTodosPorMedico(int medicoId)
    {
        return await DbContext.Horarios
            .Where(h => h.MedicoId == medicoId)
            .Include(h => h.Medico)
            .ToArrayAsync();
    }

    public async Task<HorarioModel?> BuscarPorId(int id)
    {
        return await DbContext.Horarios
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<HorarioModel> Adicionar(HorarioModel horarioModel)
    {
        horarioModel.Data = horarioModel.Data.ToUniversalTime();

        await DbContext.Horarios.AddAsync(horarioModel);
        await DbContext.SaveChangesAsync();

        return await DbContext.Horarios.FirstAsync(h => h.Id == horarioModel.Id);
    }


    public async Task Atualizar(HorarioModel horarioModel)
    {
        horarioModel.Data = horarioModel.Data.ToUniversalTime();

        DbContext.Horarios.Update(horarioModel);
        await DbContext.SaveChangesAsync();
    }


    public async Task Deletar(int id)
    {
        var horario = await BuscarPorId(id);
        if (horario != null)
        {
            DbContext.Horarios.Remove(horario);
            await DbContext.SaveChangesAsync();
        }
    }
}