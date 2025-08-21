using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vox.Application.Services;
using Vox.Application.DTOs.Horario;
using Vox.Domain.Enums;
using Vox.Infrastructure.Repositories;
using Vox.Infrastructure.Utils;

namespace Vox.API.Controllers;

[ApiController]
[Route("api")]
public class HorarioController(IHorarioService _service, IQueueManager _queueManager, IConsultaRepository _consultaRepository) : ControllerBase
{
    [HttpGet("medicos/{id}/horarios")]
    [ApiExplorerSettings(GroupName = "Medico")]
    [Authorize]
    [ProducesResponseType(typeof(HorarioOutputDTO[]), 200)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<HorarioOutputDTO[]>> GetAll(int id, [FromQuery] bool? ocupado = null)
    {
        var horariosList = (await _service.BuscaTodos(id)).ToList();
        var consultas = await _consultaRepository.BuscarPorMedico(id);

        if (ocupado.HasValue)
        {
            horariosList = horariosList
                .Where(h =>
                {
                    bool temConsultaAtiva = consultas.Any(c => c.HorarioId == h.Id && c.Status != StatusConsultaEnum.Cancelada);
                    return ocupado.Value ? temConsultaAtiva : !temConsultaAtiva;
                })
                .ToList();
        }
        
        var listaHorarios = horariosList.Select(HorarioOutputDTO.FromModel).ToArray();
        return Ok(listaHorarios);
    }

    [HttpGet("horarios/{id}")]
    [ApiExplorerSettings(GroupName = "Horarios")]
    [Authorize]
    [ProducesResponseType(typeof(HorarioOutputDTO), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<HorarioOutputDTO>> GetById(int id)
    {
        var horario = await _service.BuscarPorId(id, HttpContext.Items["Token"] as string);
        if (horario == null) return NotFound();
        return Ok(HorarioOutputDTO.FromModel(horario));
    }

    [HttpPost("horarios")]
    [ApiExplorerSettings(GroupName = "Horarios")]
    [Authorize(Roles = "Medico")]
    [ProducesResponseType(typeof(HorarioOutputDTO), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<HorarioOutputDTO>> Post([FromBody] CadastroHorarioDTO horarioDto)
    {
        try
        {
            var horario = await _queueManager.Enqueue(() => 
                _service.Adiciona(horarioDto, HttpContext.Items["Token"] as string)
            );
            
            if (horario == null) return NotFound();
            
            return Ok(HorarioOutputDTO.FromModel(horario));
        }
        catch (Exception ex)
        {
            return StatusCode(400, new {
                success = false,
                meessage = ex.Message
            });
        }
    }

    [HttpPut("horarios/{id}")]
    [ApiExplorerSettings(GroupName = "Horarios")]
    [Authorize(Roles = "Medico")]
    [ProducesResponseType(typeof(HorarioOutputDTO), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<HorarioOutputDTO>> Put(int id, [FromBody] CadastroHorarioDTO horarioDto)
    {
        try
        {
            var horario = await _queueManager.Enqueue(() => 
                _service.Edita(id, horarioDto, HttpContext.Items["Token"] as string)
            );
            
            if (horario == null) return NotFound();
            
            return Ok(HorarioOutputDTO.FromModel(horario));
        }
        catch (Exception ex)
        {
            return StatusCode(400, new {
                success = false,
                meessage = ex.Message
            });
        }
    }

    [HttpDelete("horarios/{id}")]
    [ApiExplorerSettings(GroupName = "Horarios")]
    [ProducesResponseType(typeof(HorarioOutputDTO), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<HorarioOutputDTO>> Delete(int id)
    {
        var horario = await _queueManager.Enqueue(() =>
            _service.Deleta(id, HttpContext.Items["Token"] as string)
        );
        
        if (horario == null) return NotFound();
        
        return Ok(HorarioOutputDTO.FromModel(horario));
    }
}
