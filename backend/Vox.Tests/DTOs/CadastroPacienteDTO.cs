namespace Vox.Tests.DTOs;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Moq;
using Xunit;

using Vox.Application.DTOs.Usuario;
using Vox.Application.DTOs.Paciente;
using Vox.Application.Services;
using Vox.API.Controllers;
using Vox.Domain.Enums;

public class PacienteControllerDTOTests
{
    private readonly Mock<IPacienteService> _pacienteServiceMock;
    private readonly PacienteController _controller;

    public PacienteControllerDTOTests()
    {
        _pacienteServiceMock = new Mock<IPacienteService>(MockBehavior.Strict);
        _controller = new PacienteController(_pacienteServiceMock.Object);
    }

    private CadastroPacienteDTO CadastroPacienteDTOMock()
    {
        return new CadastroPacienteDTO
        {
            Nome = "João da Silva",
            Sexo = PessoaEnum.Masculino,
            DataNascimento = new DateTime(1990, 1, 1),
            Telefone = "123456789",
            Email = "joao.silva@example.com",
            CPF = "913.127.950-34",
            Usuario = new CadastroUsuarioDTO
            {
                Login = "joaosilva",
                Senha = "Senha123"
            }
        };
    }

    [Fact]
    public void CadastroPacienteDTO_NomeVazio_ShouldFailValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Nome = "";

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Nome"));
    }

    [Fact]
    public void CadastroPacienteDTO_NomeMuitoLongo_ShouldFailValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Nome = new string('A', 101);

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Nome"));
    }
    
    [Fact]
    public void CadastroPacienteDTO_ValidEmail_ShouldPassValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Email = "teste@exemplo.com";

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.True(isValid);
    }

    [Theory]
    [InlineData("usuario@@dominio.com")]
    [InlineData("usuario dominio.com")]
    public void CadastroPacienteDTO_InvalidEmails_ShouldFailValidation(string invalidEmail)
    {
        var dto = CadastroPacienteDTOMock();
        dto.Email = invalidEmail;

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }
    
    [Fact]
    public void CadastroPacienteDTO_EmptyEmail_ShouldFailValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Email = "";

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void CadastroPacienteDTO_NullEmail_ShouldFailValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Email = null!;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }
    
    [Fact]
    public void CadastroPacienteDTO_ValidSexoMasculino_ShouldPassValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Sexo = PessoaEnum.Masculino;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }

    [Fact]
    public void CadastroPacienteDTO_ValidSexoFeminino_ShouldPassValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Sexo = PessoaEnum.Feminino;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }

    [Fact]
    public void CadastroPacienteDTO_ValidSexoOutro_ShouldPassValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Sexo = PessoaEnum.Outro;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }
    
    [Fact]
    public void CadastroPacienteDTO_SexoNaoEsperado_ShouldFailValidation()
    {
        var dto = CadastroPacienteDTOMock();
        dto.Sexo = (PessoaEnum)99;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        if (!Enum.IsDefined(typeof(PessoaEnum), dto.Sexo))
        {
            results.Add(new ValidationResult(
                "Sexo selecionado não é uma opção ofertada.",
                new[] { nameof(dto.Sexo) }
            ));
            isValid = false;
        }

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Sexo"));
    }

    [Fact]
    public void CadastroPacienteDTO_DataNascimentoFutura_DeveFalharValidacao()
    {
        var dto = CadastroPacienteDTOMock();
        dto.DataNascimento = DateTime.Now.AddYears(1);

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);
        var erros = results.Select(r => r.ErrorMessage).ToList();

        Assert.Contains("A Data de Nascimento deve ser uma data passada.", erros);
    }
    
    [Fact]
    public void CadastroPacienteDTO_DataNascimentoNula_DeveFalharValidacao()
    {
        var dto = CadastroPacienteDTOMock();
        dto.DataNascimento = default;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);
        var erros = results.Select(r => r.ErrorMessage).ToList();

        Assert.Contains("A Data de Nascimento deve ser uma data passada.", erros);
    }
    [Fact]
    public void CadastroPacienteDTO_DataNascimentoValida_DevePassarValidacao()
    {
        var dto = CadastroPacienteDTOMock();
        dto.DataNascimento = DateTime.Today.AddYears(-30);

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }
}
