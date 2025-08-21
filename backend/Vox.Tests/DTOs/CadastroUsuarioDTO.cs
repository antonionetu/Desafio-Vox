namespace Vox.Tests.DTOs;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

using Vox.Application.DTOs.Usuario;

public class CadastroUsuarioDTOTests
{
    private CadastroUsuarioDTO UsuarioMock()
    {
        return new CadastroUsuarioDTO
        {
            Login = "usuario123",
            Senha = "Senha123"
        };
    }

    [Fact]
    public void CadastroUsuarioDTO_LoginVazio_ShouldFailValidation()
    {
        var dto = UsuarioMock();
        dto.Login = "";

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Login"));
    }

    [Fact]
    public void CadastroUsuarioDTO_LoginMuitoLongo_ShouldFailValidation()
    {
        var dto = UsuarioMock();
        dto.Login = new string('a', 51);

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Login"));
    }

    [Fact]
    public void CadastroUsuarioDTO_LoginInvalido_ShouldFailValidation()
    {
        var dto = UsuarioMock();
        dto.Login = "login!inválido";

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Login"));
    }

    [Fact]
    public void CadastroUsuarioDTO_SenhaVazia_ShouldFailValidation()
    {
        var dto = UsuarioMock();
        dto.Senha = "";

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Senha"));
    }

    [Theory]
    [InlineData("1234567")] 
    [InlineData("abcdefgh")]
    [InlineData("12345678")]
    public void CadastroUsuarioDTO_SenhaInvalida_ShouldFailValidation(string senhaInvalida)
    {
        var dto = UsuarioMock();
        dto.Senha = senhaInvalida;

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Senha"));
    }

    [Fact]
    public void CadastroUsuarioDTO_SenhaValida_ShouldPassValidation()
    {
        var dto = UsuarioMock();
        dto.Senha = "Abc12345";

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, true);

        Assert.True(isValid);
    }
}
