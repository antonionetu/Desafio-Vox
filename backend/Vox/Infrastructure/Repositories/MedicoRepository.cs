namespace Vox.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using Vox.Infrastructure.Data;
using Vox.Domain.Models;

public interface IMedicoRepository
{
    IQueryable<MedicoModel> BuscarTodos(); 
    Task<MedicoModel?> BuscarPorId(int id);
    Task<MedicoModel> Adicionar(MedicoModel medicoModel);
    Task<MedicoModel?> BuscarPorUsuarioId(int usuarioId);
    Task<MedicoModel> Atualizar(MedicoModel medicoModel);
}

public class MedicoRepository(AppDbContext DbContext) : IMedicoRepository
{
    public IQueryable<MedicoModel> BuscarTodos()
    {
        return DbContext.Medicos
            .Include(m => m.Usuario)
            .AsNoTracking(); 
    }
    
    public async Task<MedicoModel?> BuscarPorId(int id)
    {
        return await DbContext.Medicos
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MedicoModel?> BuscarPorUsuarioId(int usuarioId)
    {
        return await DbContext.Medicos
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.UsuarioId == usuarioId);
    }
    
    public async Task<MedicoModel> Adicionar(MedicoModel medicoModel)
    {
        await DbContext.Medicos.AddAsync(medicoModel);
        await DbContext.SaveChangesAsync();

        return await DbContext.Medicos
            .Include(m => m.Usuario)
            .FirstAsync(m => m.Id == medicoModel.Id);
    }
    
    public async Task<MedicoModel> Atualizar(MedicoModel medicoModel)
    {
        DbContext.Medicos.Update(medicoModel);
        await DbContext.SaveChangesAsync();

        return await DbContext.Medicos
            .Include(m => m.Usuario)
            .FirstAsync(m => m.Id == medicoModel.Id);
    }
}
