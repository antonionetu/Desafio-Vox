namespace Vox.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Vox.Application.DTOs.Paciente;
using Vox.Application.Services;
using Vox.Domain.Models;

[ApiController]
[Route("api/pacientes")]
public class PacienteController(IPacienteService _service) : ControllerBase
{ 
    [HttpGet("{id}")]
    [Authorize] 
    [ProducesResponseType(typeof(PacienteOutputDTO), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PacienteModel>> GetById(int id)
    {
        var paciente = await _service.BuscarPorId(id, HttpContext.Items["Token"] as string);
    
        if (paciente == null)
            return NotFound();

        return Ok(PacienteOutputDTO.FromModel(paciente));
    }

    [HttpPost]
    public async Task<ActionResult<PacienteModel>> Post([FromBody] CadastroPacienteDTO pacienteDto)
    {
        var paciente = await _service.Adicionar(pacienteDto);
        return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, PacienteOutputDTO.FromModel(paciente));
    }
    
    [HttpPut]
    [Authorize(Roles = "Paciente")]
    public async Task<ActionResult<PacienteModel>> Put([FromBody] AtualizarPacienteDTO dto)
    {
        var paciente = await _service.Atualizar(dto, HttpContext.Items["Token"] as string);
        return Ok(PacienteOutputDTO.FromModel(paciente));
    }
}
