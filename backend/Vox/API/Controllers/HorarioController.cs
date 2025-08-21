using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vox.Application.DTOs;
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
    [Authorize]
    [ApiExplorerSettings(GroupName = "Medico")]
    [ProducesResponseType(typeof(HorarioOutputDTO[]), 200)]
    [ProducesResponseType(typeof(ErroResponseDTO), 403)]
    public async Task<ActionResult<HorarioOutputDTO[]>> GetAll(int id, [FromQuery] bool? ocupado = null)
    {
        var horarios = (await _service.BuscaTodos(id)).ToList();
        var consultas = await _consultaRepository.BuscarPorMedico(id);

        if (ocupado.HasValue)
        {
            horarios = horarios
                .Where(h =>
                {
                    bool temConsultaAtiva = consultas.Any(c => c.HorarioId == h.Id && c.Status != StatusConsultaEnum.Cancelada);
                    return ocupado.Value ? temConsultaAtiva : !temConsultaAtiva;
                })
                .ToList();
        }

        var listaHorarios = horarios.Select(HorarioOutputDTO.FromModel).ToArray();
        return Ok(listaHorarios);
    }

    [HttpGet("horarios/{id}")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Horarios")]
    [ProducesResponseType(typeof(HorarioOutputDTO), 200)]
    [ProducesResponseType(typeof(ErroResponseDTO), 403)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult<HorarioOutputDTO>> GetById(int id)
    {
        var horario = await _service.BuscarPorId(id, HttpContext.Items["Token"] as string);
        if (horario == null) return NotFound();
        return Ok(HorarioOutputDTO.FromModel(horario));
    }

    [HttpPost("horarios")]
    [Authorize(Roles = "Medico")]
    [ApiExplorerSettings(GroupName = "Horarios")]
    [ProducesResponseType(typeof(HorarioOutputDTO), 200)]
    [ProducesResponseType(typeof(ErroResponseDTO), 400)]
    [ProducesResponseType(typeof(ErroResponseDTO), 403)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult<HorarioOutputDTO>> Post([FromBody] CadastroHorarioDTO dto)
    {
        try
        {
            var horario = await _queueManager.Enqueue(() =>
                _service.Adiciona(dto, HttpContext.Items["Token"] as string)
            );

            if (horario == null) return NotFound();
            return Ok(HorarioOutputDTO.FromModel(horario));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpPut("horarios/{id}")]
    [Authorize(Roles = "Medico")]
    [ApiExplorerSettings(GroupName = "Horarios")]
    [ProducesResponseType(typeof(HorarioOutputDTO), 200)]
    [ProducesResponseType(typeof(ErroResponseDTO), 400)]
    [ProducesResponseType(typeof(ErroResponseDTO), 403)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult<HorarioOutputDTO>> Put(int id, [FromBody] CadastroHorarioDTO dto)
    {
        try
        {
            var horario = await _queueManager.Enqueue(() =>
                _service.Edita(id, dto, HttpContext.Items["Token"] as string)
            );

            if (horario == null) return NotFound();
            return Ok(HorarioOutputDTO.FromModel(horario));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpDelete("horarios/{id}")]
    [Authorize(Roles = "Medico")]
    [ApiExplorerSettings(GroupName = "Horarios")]
    [ProducesResponseType(typeof(object),200)]
    [ProducesResponseType(typeof(ErroResponseDTO), 403)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult<HorarioOutputDTO>> Delete(int id)
    {
        var horario = await _queueManager.Enqueue(() =>
            _service.Deleta(id, HttpContext.Items["Token"] as string)
        );

        if (horario == null) return NotFound();
        return Ok();
    }
}
