using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

using Vox.Application.DTOs.Usuario;
using Vox.Application.DTOs.Paciente;
using Vox.Application.Services;
using Vox.API.Controllers;
using Vox.Domain.Enums;
using Vox.Domain.Models;

namespace Vox.Tests.Controllers;

public class PacienteControllerTests
{
    private readonly Mock<IPacienteService> _pacienteServiceMock;
    private readonly PacienteController _controller;

    public PacienteControllerTests()
    {
        _pacienteServiceMock = new Mock<IPacienteService>();
        _controller = new PacienteController(_pacienteServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tipo", "Medico")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
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

    private PacienteOutputDTO PacienteOutputDTOMock(CadastroPacienteDTO dto)
    {
        return new PacienteOutputDTO
        {
            Id = 1,
            Nome = dto.Nome,
            Sexo = dto.Sexo.ToString(), 
            DataNascimento = dto.DataNascimento,
            Telefone = dto.Telefone,
            Email = dto.Email,
            CPF = dto.CPF
        };
    }

    private PacienteModel PacienteModelMock(CadastroPacienteDTO dto)
    {
        return new PacienteModel
        {
            Nome = dto.Nome,
            Sexo = dto.Sexo,
            DataNascimento = dto.DataNascimento,
            Telefone = dto.Telefone,
            Email = dto.Email,
            CPF = dto.CPF,
            Id = 1,
            Usuario = new UsuarioModel 
            {
                Id = 1,
                Login = dto.Usuario.Login,
                Senha = dto.Usuario.Senha,
                Tipo = TipoUsuarioEnum.Paciente
            }
        };
    }

    [Fact]
    public async Task GetById_PacienteEncontrado_TokenValido()
    {
        var dto = CadastroPacienteDTOMock();
        var pacienteOutput = PacienteOutputDTOMock(dto);
        var pacienteModel = PacienteModelMock(dto);

        _pacienteServiceMock
            .Setup(s => s.BuscarPorId(pacienteOutput.Id, It.IsAny<string>()))
            .ReturnsAsync(pacienteModel);

        var result = await _controller.GetById(pacienteOutput.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var retorno = Assert.IsType<PacienteOutputDTO>(okResult.Value);
        Assert.Equal(pacienteOutput.CPF, retorno.CPF);
    }

    [Fact]
    public async Task GetById_PacienteNaoEncontrado_TokenValido_DeveRetornarNotFound()
    {
        _pacienteServiceMock
            .Setup(s => s.BuscarPorId(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((PacienteModel)null!);

        var result = await _controller.GetById(-1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Post_PacienteValido_TokenValido_DeveRetornarCreated()
    {
        var dto = CadastroPacienteDTOMock();
        var pacienteCriadoModel = PacienteModelMock(dto);

        _pacienteServiceMock
            .Setup(s => s.Adicionar(dto))
            .ReturnsAsync(pacienteCriadoModel);

        var result = await _controller.Post(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("GetById", createdResult.ActionName);
        var retorno = Assert.IsType<PacienteOutputDTO>(createdResult.Value);
        Assert.Equal(dto.CPF, retorno.CPF);
    }
}
