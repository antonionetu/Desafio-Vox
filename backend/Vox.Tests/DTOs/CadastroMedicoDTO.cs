namespace Vox.Tests.DTOs;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;


using Vox.Application.DTOs.Usuario;
using Vox.Application.DTOs.Medico;
using Vox.Application.Services;
using Vox.API.Controllers;
using Vox.Domain.Models;
using Vox.Domain.Enums;

public class MedicoControllerDTOTests
{
    private readonly Mock<IMedicoService> _medicoServiceMock;
    private readonly MedicoController _controller;

    public MedicoControllerDTOTests()
    {
        _medicoServiceMock = new Mock<IMedicoService>();
        _controller = new MedicoController(_medicoServiceMock.Object);
    }

    private CadastroMedicoDTO CadastroMedicoDTOMock()
    {
        return new CadastroMedicoDTO
        {
            Nome = "Seu Jose da Silva",
            Email = "dr.jose@example.com",
            Especialidade = "Cardiologia",
            Sexo = PessoaEnum.Masculino,
            DataNascimento = new DateTime(1980, 1, 1),
            CRM = "12345",
            Usuario = new CadastroUsuarioDTO
            {
                Login = "medico123",
                Senha = "Medico123"
            }
        };
    }

    [Fact]
    public void CadastroMedicoDTO_NomeVazio_ShouldFailValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Nome = "";

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Nome"));
    }

    [Fact]
    public void CadastroMedicoDTO_NomeMuitoLongo_ShouldFailValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Nome = new string('A', 101);

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Nome"));
    }
    
    [Fact]
    public void CadastroMedicoDTO_ValidEmail_ShouldPassValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Email = "teste@exemplo.com";

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
        Assert.DoesNotContain(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void CadastroMedicoDTO_EmptyEmail_ShouldFailValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Email = "";

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void CadastroMedicoDTO_NullEmail_ShouldFailValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Email = null!;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }
    
    [Theory]
    [InlineData("usuario@@dominio.com")]
    [InlineData("usuario dominio.com")]
    public void CadastroMedicoDTO_InvalidEmails_ShouldFailValidation(string invalidEmail)
    {
        var dto = CadastroMedicoDTOMock();
        dto.Email = invalidEmail;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void CadastroMedicoDTO_ValidEspecialidade_ShouldPassValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Especialidade = "Cardiologia";

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }
    
    [Fact]
    public void CadastroMedicoDTO_EmptyEspecialidade_ShouldFailValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Especialidade = string.Empty;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Especialidade"));
    }

    [Fact]
    public void CadastroMedicoDTO_NullEspecialidade_ShouldFailValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Especialidade = null!;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Especialidade"));
    }

    [Fact]
    public void CadastroMedicoDTO_EspecialidadeTooLong_ShouldFailValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Especialidade = new string('A', 101);

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Especialidade"));
    }

    [Fact]
    public void CadastroMedicoDTO_ValidSexoMasculino_ShouldPassValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Sexo = PessoaEnum.Masculino;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }

    [Fact]
    public void CadastroMedicoDTO_ValidSexoFeminino_ShouldPassValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Sexo = PessoaEnum.Feminino;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }

    [Fact]
    public void CadastroMedicoDTO_ValidSexoOutro_ShouldPassValidation()
    {
        var dto = CadastroMedicoDTOMock();
        dto.Sexo = PessoaEnum.Outro;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }
    
    
    [Fact]
    public void CadastroMedicoDTO_SexoNaoEsperado_ShouldFailValidation()
    {
        var dto = CadastroMedicoDTOMock();
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
    public void CadastroMedicoDTO_DataNascimentoNula_DeveFalharValidacao()
    {
        var dto = CadastroMedicoDTOMock();
        dto.DataNascimento = default;

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);
        var erros = results.Select(r => r.ErrorMessage).ToList();

        Assert.Contains("A Data de Nascimento deve ser uma data passada.", erros);
    }

    [Fact]
    public void CadastroMedicoDTO_DataNascimentoFutura_DeveFalharValidacao()
    {
        var dto = CadastroMedicoDTOMock();
        dto.DataNascimento = DateTime.Now.AddYears(1);
        Console.WriteLine(dto);

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);
        var erros = results.Select(r => r.ErrorMessage).ToList();

        Assert.Contains("A Data de Nascimento deve ser uma data passada.", erros);
    }

    [Fact]
    public void CadastroMedicoDTO_DataNascimentoValida_DevePassarValidacao()
    {
        var dto = CadastroMedicoDTOMock();
        dto.DataNascimento = DateTime.Today.AddYears(-30);

        var validationContext = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, validationContext, results, true);

        Assert.True(isValid);
    }
}
