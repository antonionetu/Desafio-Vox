namespace Vox.Domain.Models;

using System.ComponentModel.DataAnnotations;

using Domain.Enums;

public class UsuarioModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O login é obrigatório.")]
    [StringLength(50, ErrorMessage = "O login não pode ter mais de 50 caracteres.")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória.")] 
    [StringLength(100, ErrorMessage = "A senha não pode ter mais de 100 caracteres.")]
    public string Senha { get; set; } = null!;

    [Required]
    public string Salt { get; set; } = null!;

    [Required]
    public TipoUsuarioEnum Tipo { get; set; }
}