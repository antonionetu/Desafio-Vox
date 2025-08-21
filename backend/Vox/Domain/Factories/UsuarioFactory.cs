namespace Vox.Domain.Factories;

using Vox.Domain.Models;
using Domain.Enums;

public class UsuarioFactory
{
    public UsuarioModel Criar(string login, string senhaHash, string salt, TipoUsuarioEnum tipo)
    {
        var usuario = new UsuarioModel
        {
            Login = login,
            Senha = senhaHash,
            Salt = salt,
            Tipo = tipo
        };

        return usuario;
    }
}