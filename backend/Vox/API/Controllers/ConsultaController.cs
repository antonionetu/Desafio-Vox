namespace Vox.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Vox.Application.Services;
using Vox.Application.DTOs.Consulta;
using Vox.Infrastructure.Utils;
using Vox.Domain.Models;
using Vox.Domain.Enums;

[ApiController]
[Route("api")]
public class ConsultaController(IConsultaService _service, IQueueManager _queueManager) : ControllerBase
{
    [HttpGet("medicos/{id}/consultas")]
    [Authorize(Roles = "Medico")]
    [ApiExplorerSettings(GroupName = "Medico")]
    [ProducesResponseType(typeof(ConsultaOutputDTO[]), 200)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ConsultaModel[]>> GetAllMedico([FromQuery] StatusConsultaEnum? status)
    {
        var consultas = await _service.BuscarTodosPorMedico(status, HttpContext.Items["Token"] as string);
        var listaConsultas = consultas.Select(ConsultaOutputDTO.FromModel).ToArray();
        return Ok(listaConsultas);
    }
    
    [HttpGet("pacientes/{id}/consultas")]
    [Authorize(Roles = "Paciente")]
    [ApiExplorerSettings(GroupName = "Paciente")]
    [ProducesResponseType(typeof(ConsultaOutputDTO[]), 200)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ConsultaModel[]>> GetAllPaciente(StatusConsultaEnum? status)
    {
        var consultas = await _service.BuscarTodosPorPaciente(status, HttpContext.Items["Token"] as string);
        var listaConsultas = consultas.Select(ConsultaOutputDTO.FromModel).ToArray();
        return Ok(listaConsultas);
    }

    [HttpGet("consultas/{id}")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Consulta")]
    [ProducesResponseType(typeof(ConsultaOutputDTO), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ConsultaModel>> GetById(int id)
    {
        var consulta = await _service.BuscarPorId(id, HttpContext.Items["Token"] as string);
        if (consulta == null) return NotFound();
        return Ok(ConsultaOutputDTO.FromModel(consulta));
    }

    [HttpPost("consultas")]
    [Authorize(Roles = "Paciente")]
    [ApiExplorerSettings(GroupName = "Consulta")]
    [ProducesResponseType(typeof(ConsultaOutputDTO), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ConsultaModel>> Post([FromBody] CadastroConsultaDTO consultaDto)
    {
        try
        {
            var consulta = await _queueManager.Enqueue(() =>
                _service.Adicionar(consultaDto, HttpContext.Items["Token"] as string)
            );
            
            return Ok(new {
                success = true,
                message = "Consulta criada com sucesso!",
                data = consulta
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex) 
        {
            return StatusCode(500, new {
                success = false,
                message = "Ocorreu um erro inesperado.",
                details = ex.Message
            });
        }
    }

    [HttpPatch("consultas/{id}/status")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Consulta")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> PatchStatus(int id, [FromBody] AtualizarStatusDTO dto)
    {
        try
        {
            var consulta = await _queueManager.Enqueue(() =>
                _service.AtualizarStatus(id, dto.Status, HttpContext.Items["Token"] as string)
            );

            if (consulta == null) 
                return NotFound(new { success = false, message = "Consulta não encontrada." });

            return Ok(new {
                success = true,
                horario = consulta.Horario
            });
        }
        catch (Exception ex)
        {
            return StatusCode(400, new {
                success = false,
                message = ex.Message
            });
        }
    }
}
