using Microsoft.EntityFrameworkCore;
using Vox.Infrastructure.Data;
using Vox.Domain.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Vox.Infrastructure.Repositories
{
    public interface INotificacaoRepository
    {
        IQueryable<NotificacaoModel> BuscarTodos();
        Task<NotificacaoModel> Adicionar(NotificacaoModel notificacao);
    }

    public class NotificacaoRepository : INotificacaoRepository
    {
        private readonly AppDbContext _dbContext;

        public NotificacaoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<NotificacaoModel> BuscarTodos()
        {
            return _dbContext.Notificacoes
                .Include(n => n.Consulta)
                .AsNoTracking();
        }

        public async Task<NotificacaoModel> Adicionar(NotificacaoModel notificacao)
        {
            await _dbContext.Notificacoes.AddAsync(notificacao);
            await _dbContext.SaveChangesAsync();

            return await _dbContext.Notificacoes
                .Include(n => n.Consulta)
                .FirstAsync(n => n.Id == notificacao.Id);
        }
    }
}
