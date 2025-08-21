using Vox.Domain.Models;

namespace Vox.Application.DTOs.Paciente;

using System;
using System.ComponentModel.DataAnnotations;

using Vox.Application.DTOs.Usuario;
using Vox.Application.Validators;
using Vox.Domain.Enums;

public class CadastroPacienteDTO
{
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O Nome deve ter no máximo {1} caracteres.")]
    [NomeValido(ErrorMessage = "O Nome informado é inválido.")]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "O campo Sexo é obrigatório.")]
    [SexoValido(ErrorMessage = "O Sexo informado é inválido.")]
    public PessoaEnum Sexo { get; set; }

    [Required(ErrorMessage = "O campo Data de Nascimento é obrigatório.")]
    [DataType(DataType.Date)]
    [DataNascimentoValida(ErrorMessage = "A Data de Nascimento deve ser uma data passada.")]
    public DateTime DataNascimento { get; set; }

    [Required(ErrorMessage = "O campo Telefone é obrigatório.")]
    [Phone(ErrorMessage = "O formato do telefone é inválido.")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo {1} caracteres.")]
    public string Telefone { get; set; } = null!;

    [Required(ErrorMessage = "O campo Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do email é inválido.")]
    [StringLength(100, ErrorMessage = "O email deve ter no máximo {1} caracteres.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "O campo CPF é obrigatório.")]
    [CpfValido(ErrorMessage = "O CPF informado é inválido.")]
    public string CPF { get; set; } = null!;

    [Required(ErrorMessage = "O campo Usuario é obrigatório.")]
    public CadastroUsuarioDTO Usuario { get; set; } = null!;
}

public class PacienteOutputDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Sexo { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public UsuarioOutputDTO Usuario { get; set; }

    public static PacienteOutputDTO FromModel(PacienteModel paciente)
    {
        return new PacienteOutputDTO
        {
            Id = paciente.Id,
            Nome = paciente.Nome,
            Sexo = paciente.Sexo.ToString(),
            DataNascimento = paciente.DataNascimento,
            Telefone = paciente.Telefone,
            Email = paciente.Email,
            CPF = paciente.CPF,
            Usuario = paciente.Usuario != null
                ? new UsuarioOutputDTO
                {
                    Id = paciente.Usuario.Id,
                    Tipo = paciente.Usuario.Tipo
                }
                : null!
        };
    }
}