namespace Vox.Application.DTOs.Paciente;

using System;
using System.ComponentModel.DataAnnotations;

public class AtualizarPacienteDTO
{
    [StringLength(100)]
    public string? Nome { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DataNascimento { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Telefone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
}