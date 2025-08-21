namespace Vox.Application.Services;

using Microsoft.AspNetCore.SignalR;

using Vox.API.Hubs;
using Vox.Infrastructure.Repositories;
using Vox.Domain.Models;

public interface INotificacaoService
{
    Task<NotificacaoModel> CriarNotificacao(string mensagem, ConsultaModel consulta);
}

public class NotificacaoService : INotificacaoService
{
    private readonly IHubContext<NotificacaoHub> _hubContext;
    private readonly INotificacaoRepository _repository;

    public NotificacaoService(IHubContext<NotificacaoHub> hubContext, INotificacaoRepository repository)
    {
        _hubContext = hubContext;
        _repository = repository;
    }

    public async Task<NotificacaoModel> CriarNotificacao(string mensagem, ConsultaModel consulta)
    {
        var notificacao = new NotificacaoModel
        {
            Mensagem = mensagem,
            Data = DateTime.UtcNow,
            Hora = DateTime.UtcNow.TimeOfDay,
            Consulta = consulta,
            ConsultaId = consulta.Id
        };

        await _repository.Adicionar(notificacao);

        await _hubContext.Clients.User(consulta.Horario.MedicoId.ToString())
            .SendAsync("ReceberNotificacao", notificacao);

        await _hubContext.Clients.User(consulta.PacienteId.ToString())
            .SendAsync("ReceberNotificacao", notificacao);

        return notificacao;
    }
}
