namespace Vox.Domain.Factories;

using Vox.Application.DTOs.Medico;
using Vox.Domain.Models;

public class MedicoFactory
{
    public MedicoModel Criar(CadastroMedicoDTO dto, UsuarioModel usuario)
    {
        var dataNascimentoUtc = DateTime.SpecifyKind(dto.DataNascimento, DateTimeKind.Utc);

        return new MedicoModel
        {
            Nome = dto.Nome,
            Especialidade = dto.Especialidade,
            Sexo = dto.Sexo,
            DataNascimento = dataNascimentoUtc,
            Telefone = dto.Telefone,
            Email = dto.Email,
            CRM = dto.CRM,
            Descricao = dto.Descricao,
            Usuario = usuario,
            UsuarioId = usuario.Id
        };
    }
}
