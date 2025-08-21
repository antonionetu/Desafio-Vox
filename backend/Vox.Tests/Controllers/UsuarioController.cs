namespace Vox.Tests.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;

using Vox.Application.DTOs.Usuario;
using Vox.API.Controllers;
using Vox.Application.Services; 
using Vox.Domain.Enums;
using Vox.Domain.Models;

public class UsuarioControllerTests
{
    private readonly Mock<IUsuarioService> _usuarioServiceMock;
    private readonly UsuarioController _controller;

    public UsuarioControllerTests()
    {
        _usuarioServiceMock = new Mock<IUsuarioService>();
        _controller = new UsuarioController(_usuarioServiceMock.Object);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        var loginDto = new LoginDTO { Login = "teste", Senha = "1234" };
        
        _usuarioServiceMock.Setup(s => s.VerificarAcesso(loginDto))
            .ReturnsAsync((UsuarioModel?)null);

        var result = await _controller.Login(loginDto);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Login e/ou senha incorretos", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOkComToken()
    {
        var loginDto = new LoginDTO { Login = "teste", Senha = "1234" };
        var usuario = new UsuarioModel { Id = 1, Tipo = TipoUsuarioEnum.Paciente, Login = "teste" };
        var tokenGerado = "token123";

        _usuarioServiceMock.Setup(s => s.VerificarAcesso(loginDto))
            .ReturnsAsync(usuario);
            
        _usuarioServiceMock.Setup(s => s.GerarTokenJWT(usuario))
            .Returns(tokenGerado);

        var result = await _controller.Login(loginDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var loginOutput = Assert.IsType<LoginOutputDTO>(okResult.Value);
        Assert.Equal(tokenGerado, loginOutput.Token);
    }
}