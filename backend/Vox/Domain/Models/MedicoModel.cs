namespace Vox.Domain.Models;

using System.ComponentModel.DataAnnotations;

using Vox.Domain.Enums;

// herdar de baseEntity
public class MedicoModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
    public string Nome { get; set; }
    
    [Required(ErrorMessage = "Um médico deve ter uma Especialidade.")]
    [StringLength(100, ErrorMessage = "Especialidade deve ter no máximo 100 caracteres.")]
    public string Especialidade { get; set; }

    [Required(ErrorMessage = "O sexo é obrigatório.")]
    public PessoaEnum Sexo { get; set; }

    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    [DataType(DataType.Date)]
    public DateTime DataNascimento { get; set; }

    [Phone(ErrorMessage = "Telefone inválido.")]
    [StringLength(20, ErrorMessage = "O telefone não pode ter mais de 20 caracteres.")]
    public string Telefone { get; set; }

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(100, ErrorMessage = "O e-mail não pode ter mais de 100 caracteres.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [StringLength(10, ErrorMessage = "O CRM deve ter no máximo 10 caracteres.")]
    public string CRM { get; set; }
    
    [StringLength(500, ErrorMessage = "A descrição não pode ter mais de 500 caracteres.")]
    public string? Descricao  { get; set; }
    
    [Required]
    public UsuarioModel Usuario { get; set; } = null!;
    public int UsuarioId { get; set; }
}