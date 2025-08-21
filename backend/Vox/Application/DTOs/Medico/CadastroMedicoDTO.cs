
namespace Vox.Application.DTOs.Medico;

using System;
using System.ComponentModel.DataAnnotations;

using Vox.Application.DTOs.Usuario;
using Vox.Application.Validators;
using Vox.Domain.Enums;

public class CadastroMedicoDTO
{
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O Nome deve ter no máximo {1} caracteres.")]
    [NomeValido(ErrorMessage = "O Nome informado é inválido.")]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "O campo Especialidade é obrigatório.")]
    [StringLength(100, ErrorMessage = "A Especialidade deve ter no máximo {1} caracteres.")]
    public string Especialidade { get; set; } = null!;

    [Required(ErrorMessage = "O campo Sexo é obrigatório.")]
    [SexoValido(ErrorMessage = "O Sexo informado é inválido.")]
    public PessoaEnum Sexo { get; set; }

    [Required(ErrorMessage = "O campo Data de Nascimento é obrigatório.")]
    [DataType(DataType.Date)]
    [DataNascimentoValida(ErrorMessage = "A Data de Nascimento deve ser uma data passada.")]
    public DateTime DataNascimento { get; set; }

    [Phone(ErrorMessage = "O formato do telefone é inválido.")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo {1} caracteres.")]
    public string? Telefone { get; set; }

    [Required(ErrorMessage = "O campo Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do email é inválido.")]
    [StringLength(100, ErrorMessage = "O email deve ter no máximo {1} caracteres.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "O campo CRM é obrigatório.")]
    [StringLength(10, ErrorMessage = "O CRM deve ter no máximo {1} caracteres.")]
    public string CRM { get; set; } = null!;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo {1} caracteres.")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "O campo Usuario é obrigatório.")]
    public CadastroUsuarioDTO Usuario { get; set; } = null!;
}
