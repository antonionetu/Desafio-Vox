namespace Vox.Domain.Factories;

using Vox.Application.DTOs.Paciente;
using Vox.Domain.Models;

public class PacienteFactory
{
    public PacienteModel Criar(CadastroPacienteDTO dto, UsuarioModel usuario)
    {
        var dataNascimentoUtc = DateTime.SpecifyKind(dto.DataNascimento, DateTimeKind.Utc);
        
        var paciente = new PacienteModel
        {
            Nome = dto.Nome,
            Sexo = dto.Sexo,
            DataNascimento = dataNascimentoUtc,
            Telefone = dto.Telefone,
            Email = dto.Email,
            CPF = dto.CPF,
            UsuarioId = usuario.Id,
            Usuario = usuario,
        };

        return paciente;
    }
}
