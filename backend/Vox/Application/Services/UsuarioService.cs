namespace Vox.Application.Services;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using Vox.Application.DTOs.Usuario;
using Vox.Infrastructure.Repositories;
using Vox.Infrastructure.Data;
using Vox.Domain.Models;
using Vox.Domain.Factories;
using Vox.Application.Handlers;

public interface IUsuarioService
{
    Task<UsuarioModel?> BuscarPorId(int id);
    Task<UsuarioModel> Adicionar(CadastroUsuarioDTO dto);
    Task<UsuarioModel?> VerificarAcesso(LoginDTO acesso);
}

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly UsuarioFactory _factory;
    private readonly AutenticacaoHandler _authHandler;

    public UsuarioService(
        IUsuarioRepository repository,
        UsuarioFactory factory,
        AutenticacaoHandler authHandler
    )
    {
        _repository = repository;
        _factory = factory;
        _authHandler = authHandler;
    }

    public async Task<UsuarioModel?> BuscarPorId(int id)
        => await _repository.BuscarPorId(id);

    public async Task<UsuarioModel> Adicionar(CadastroUsuarioDTO dto)
    {
        var senhaCriptografada = _authHandler.CriarHash(dto.Senha, out var salt);
        var usuario = _factory.Criar(dto.Login, senhaCriptografada, salt, dto.TipoUsuario);
        return await _repository.Adicionar(usuario);
    }

    public async Task<UsuarioModel?> VerificarAcesso(LoginDTO acesso)
    {
        var usuario = await _repository.BuscarPorLogin(acesso.Login);

        if (usuario == null)
            return null;

        var hashGerado = _authHandler.CompararHash(acesso.Senha, usuario.Salt);
        return hashGerado == usuario.Senha ? usuario : null;
    }
}
