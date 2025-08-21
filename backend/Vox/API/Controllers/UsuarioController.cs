using Microsoft.AspNetCore.Authorization;

namespace Vox.API.Controllers;

using Microsoft.AspNetCore.Mvc;

using Vox.Application.DTOs.Usuario;
using Vox.Application.Services;
using Vox.Application.Handlers;

[ApiController]
[Route("api/autenticacao")]
public class UsuarioController(IUsuarioService _service, AutenticacaoHandler _autenticacaoHandler) : ControllerBase
{ 
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginOutputDTO>> Login([FromBody] LoginDTO acesso)
    {
        var usuario = await _service.VerificarAcesso(acesso);
    
        if (usuario == null)
            return Unauthorized(new {
                Erro = "Login e/ou senha incorretos"
            });

        var token = _autenticacaoHandler.GerarTokenJWT(usuario);
        var result = new LoginOutputDTO
        {
            Token = token.Result,
            Tipo = usuario.Tipo
        };

        return Ok(result);
    }
}