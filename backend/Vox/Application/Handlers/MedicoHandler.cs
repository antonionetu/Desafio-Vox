namespace Vox.Application.Handlers;

using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

using Vox.Infrastructure.Repositories;
using Vox.Domain.Models;

public class MedicoHandler
{
    private readonly IMedicoRepository _repository;

    public MedicoHandler(IMedicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<MedicoModel?> GetMedicoFromToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        token = token.Replace("Bearer ", "").Trim();
        var handler = new JwtSecurityTokenHandler();

        JwtSecurityToken jwtToken;
        try
        {
            jwtToken = handler.ReadJwtToken(token);
        }
        catch
        {
            return null;
        }

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "usuarioId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;

        return await _repository.BuscarPorUsuarioId(userId);
    }
}