namespace Vox.Domain.Models;

using System.ComponentModel.DataAnnotations;

public class NotificacaoModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(500, ErrorMessage = "A mensagem não pode ter mais que 500 caracteres.")]
    public string Mensagem { get; set; }
    
    [Required]
    public DateTime Data { get; set; } = DateTime.Now;
    
    [Required]
    [DataType(DataType.Time)]
    public TimeSpan Hora { get; set; } =  new TimeSpan();
    
    [Required]
    public ConsultaModel Consulta { get; set; } = null!;
    public int ConsultaId { get; set; }
}