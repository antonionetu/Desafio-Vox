using Vox.Domain.Enums;

namespace Vox.Application.DTOs.Usuario;

public class LoginDTO
{
    public string Login { get; set; } = null!;
    public string Senha { get; set; } = null!;
}

public class LoginOutputDTO
{
    public string Token { get; set; } = null!;
    public TipoUsuarioEnum Tipo { get; set; }
}
