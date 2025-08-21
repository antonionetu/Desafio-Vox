namespace Vox.Infrastructure.Repositories;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Vox.Infrastructure.Data;
using Vox.Domain.Models;

public interface IPacienteRepository
{
    Task<PacienteModel?> BuscarPorId(int id);
    Task<PacienteModel?> BuscarPorUsuarioId(int usuarioId);
    Task<PacienteModel> Adicionar(PacienteModel paciente);
    Task<PacienteModel> Atualizar(PacienteModel paciente);
}

public class PacienteRepository(AppDbContext DbContext): IPacienteRepository
{
    public async Task<PacienteModel?> BuscarPorId(int id)
    {
        return await DbContext.Pacientes
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PacienteModel?> BuscarPorUsuarioId(int usuarioId)
    {
        return await DbContext.Pacientes
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
    }

    public async Task<PacienteModel> Adicionar(PacienteModel paciente)
    {
        await DbContext.Pacientes.AddAsync(paciente);
        await DbContext.SaveChangesAsync();

        return await DbContext.Pacientes
            .Include(p => p.Usuario)
            .FirstAsync(p => p.Id == paciente.Id);
    }
    
    public async Task<PacienteModel> Atualizar(PacienteModel paciente)
    {
        DbContext.Pacientes.Update(paciente);
        await DbContext.SaveChangesAsync();
        return await DbContext.Pacientes
            .Include(p => p.Usuario)
            .FirstAsync(p => p.Id == paciente.Id);
    }
}