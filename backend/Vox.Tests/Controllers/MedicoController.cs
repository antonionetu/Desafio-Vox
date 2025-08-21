namespace Vox.Tests.Controllers;

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

public class MedicoControllerTests
{
    private readonly Mock<IMedicoService> _medicoServiceMock;
    private readonly MedicoController _controller;

    public MedicoControllerTests()
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

    private MedicoOutputDTO MedicoOutputDTOMock(MedicoModel medicoModel)
    {
        return new MedicoOutputDTO
        {
            Id = medicoModel.Id,
            Nome = medicoModel.Nome,
            Email = medicoModel.Email,
            Especialidade = medicoModel.Especialidade,
            Sexo = medicoModel.Sexo.ToString(),
            DataNascimento = medicoModel.DataNascimento,
            CRM = medicoModel.CRM
        };
    }

    private MedicoModel MedicoModelMock(CadastroMedicoDTO medicoDto)
    {
        return new MedicoModel
        {
            Id = 1,
            Nome = medicoDto.Nome,
            Email = medicoDto.Email,
            Especialidade = medicoDto.Especialidade,
            Sexo = medicoDto.Sexo,
            DataNascimento = medicoDto.DataNascimento,
            CRM = medicoDto.CRM,
            Usuario = new UsuarioModel
            {
                Login = medicoDto.Usuario.Login,
                Senha = medicoDto.Usuario.Senha
            }
        };
    }

    [Fact]
    public async Task GetById_MedicoEncontrado_DeveRetornarOkComMedicoDTO()
    {
        var id = 1;
        var dto = CadastroMedicoDTOMock();
        var expectedMedicoModel = MedicoModelMock(dto);
        var expectedMedicoOutputDto = MedicoOutputDTOMock(expectedMedicoModel);

        _medicoServiceMock
            .Setup(s => s.BuscarPorId(id))
            .ReturnsAsync(expectedMedicoModel);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var retornoMedico = Assert.IsType<MedicoOutputDTO>(okResult.Value);
        
        Assert.Equal(expectedMedicoOutputDto.CRM, retornoMedico.CRM);
        Assert.Equal(expectedMedicoOutputDto.Nome, retornoMedico.Nome);
        Assert.Equal(expectedMedicoOutputDto.Especialidade, retornoMedico.Especialidade);
        Assert.Equal(expectedMedicoOutputDto.Sexo, retornoMedico.Sexo);
    }

    [Fact]
    public async Task GetById_MedicoNaoEncontrado_DeveRetornarNotFound()
    {
        _medicoServiceMock
            .Setup(s => s.BuscarPorId(It.IsAny<int>()))
            .ReturnsAsync((MedicoModel?)null);

        var result = await _controller.GetById(-1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Post_MedicoValido_DeveRetornarCreated()
    {
        var dto = CadastroMedicoDTOMock();
        var medicoCriado = MedicoModelMock(dto);
        var expectedMedicoOutputDto = MedicoOutputDTOMock(medicoCriado);

        _medicoServiceMock
            .Setup(s => s.Adicionar(dto))
            .ReturnsAsync(medicoCriado);

        var result = await _controller.Post(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("GetById", createdResult.ActionName);
        
        var retornoMedico = Assert.IsType<MedicoOutputDTO>(createdResult.Value);
        
        Assert.Equal(expectedMedicoOutputDto.CRM, retornoMedico.CRM);
        Assert.Equal(expectedMedicoOutputDto.Nome, retornoMedico.Nome);
        Assert.Equal(expectedMedicoOutputDto.Especialidade, retornoMedico.Especialidade);
        Assert.Equal(expectedMedicoOutputDto.Sexo, retornoMedico.Sexo);
    }
}