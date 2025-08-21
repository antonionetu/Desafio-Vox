namespace Vox.Infrastructure.Repositories;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Vox.Infrastructure.Data;
using Vox.Domain.Models;

public interface IUsuarioRepository
{
    Task<UsuarioModel?> BuscarPorId(int id);
    Task<UsuarioModel?> BuscarPorLogin(string login);
    Task<UsuarioModel> Adicionar(UsuarioModel usuarioModel);
}

public class UsuarioRepository(AppDbContext DbContext) : IUsuarioRepository
{
    public async Task<UsuarioModel?> BuscarPorId(int id)
    {
        return await DbContext.Usuarios.FindAsync(id);
    }

    public async Task<UsuarioModel?> BuscarPorLogin(string login)
    {
        return await DbContext.Usuarios
            .FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<UsuarioModel> Adicionar(UsuarioModel usuarioModel)
    {
        var usuario = await DbContext.Usuarios.AddAsync(usuarioModel);
        await DbContext.SaveChangesAsync();
        return usuario.Entity;
    }
}