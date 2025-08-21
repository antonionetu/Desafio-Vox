using Microsoft.AspNetCore.Authorization;

namespace Vox.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Vox.Application.DTOs;
using Vox.Application.DTOs.Medico;
using Vox.Application.Services;
using Vox.Domain.Models;

[ApiController]
[Route("api/medicos")]
public class MedicoController(IMedicoService service) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<MedicoOutputDTO>>> GetAll(
        [FromQuery] int limit = 10,
        [FromQuery] string? nome = null,
        [FromQuery] string? especialidade = null)
    {
        var result = await service.BuscarTodos(limit, nome, especialidade);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ErroResponseDTO), 403)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult<MedicoModel>> GetById(int id)
    {
        var medico = await service.BuscarPorId(id);

        if (medico == null)
            return NotFound();

        return Ok(MedicoOutputDTO.FromModel(medico));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(MedicoOutputDTO), 200)]
    [ProducesResponseType(typeof(ErroResponseDTO), 400)]
    public async Task<ActionResult<MedicoModel>> Post([FromBody] CadastroMedicoDTO medicoDto)
    {
        try
        {
            var medico = await service.Adicionar(medicoDto);
            return CreatedAtAction(nameof(GetById), new { id = medico.Id }, MedicoOutputDTO.FromModel(medico));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new {
                Erro = ex.Message
            });
        }
    }
    
    [HttpPut]
    [Authorize(Roles = "Medico")]
    [ProducesResponseType(typeof(MedicoOutputDTO), 200)]
    [ProducesResponseType(typeof(ErroResponseDTO), 400)]
    [ProducesResponseType(typeof(ErroResponseDTO), 403)]
    public async Task<ActionResult<MedicoModel>> Put([FromBody] AtualizaMedicoDTO medicoDto)
    {
        try
        {
            var medicoAtualizado = await service.Atualizar(medicoDto, HttpContext.Items["Token"] as string);
            return Ok(MedicoOutputDTO.FromModel(medicoAtualizado));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new {
                Erro = ex.Message
            });
        }
        
    }
}
