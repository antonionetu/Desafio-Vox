namespace Vox.API.Hubs;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class NotificacaoHub : Hub
{
    public async Task EnviarMensagem(string usuario, string mensagem)
    {
        await Clients.User(usuario).SendAsync("ReceberNotificacao", mensagem);
    }
}
