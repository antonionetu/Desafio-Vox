namespace Vox.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

using Vox.Domain.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UsuarioModel> Usuarios { get; set; }
    public DbSet<PacienteModel> Pacientes { get; set; }
    public DbSet<MedicoModel> Medicos { get; set; }
    public DbSet<HorarioModel> Horarios { get; set; }
    public DbSet<ConsultaModel> Consultas { get; set; }
    public DbSet<NotificacaoModel> Notificacoes { get; set; }
}
