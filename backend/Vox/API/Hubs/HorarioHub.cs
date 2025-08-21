using Microsoft.AspNetCore.Authorization;

namespace Vox.API.Hubs;

using Microsoft.AspNetCore.SignalR;

[Authorize]
public sealed class HorarioHub : Hub
{
}
