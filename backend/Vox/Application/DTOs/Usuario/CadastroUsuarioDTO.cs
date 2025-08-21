namespace Vox.Application.DTOs.Usuario;

using Vox.Domain.Enums;
using System.ComponentModel.DataAnnotations;

public class CadastroUsuarioDTO
{
    [Required(ErrorMessage = "O login é obrigatório.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O login deve ter entre 3 e 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "O login só pode conter letras e números.")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$", 
    ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma minúscula e um número.")]
    public string Senha { get; set; } = null!;
    
    [Required(ErrorMessage = "O tipo de usuário não foi informado")]
    public TipoUsuarioEnum  TipoUsuario { get; set; }
}

public class UsuarioOutputDTO
{
    public int Id { get; set; }
    public TipoUsuarioEnum Tipo { get; set; }
}
