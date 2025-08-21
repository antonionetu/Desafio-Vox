namespace Vox.Application.DTOs.Medico;

using Vox.Application.DTOs.Usuario;
using Vox.Domain.Models;

public class MedicoOutputDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Especialidade { get; set; }
    public string Sexo { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? Telefone { get; set; }
    public string Email { get; set; }
    public string CRM { get; set; }
    public string? Descricao { get; set; }
    public UsuarioOutputDTO? Usuario { get; set; }

    public static MedicoOutputDTO FromModel(MedicoModel medico)
    {
        var dto = new MedicoOutputDTO
        {
            Id = medico.Id,
            Nome = medico.Nome,
            Especialidade = medico.Especialidade,
            Sexo = medico.Sexo.ToString(),
            DataNascimento = medico.DataNascimento,
            Telefone = medico.Telefone,
            Email = medico.Email,
            CRM = medico.CRM,
            Descricao = medico.Descricao
        };

        if (medico.Usuario != null)
        {
            dto.Usuario = new UsuarioOutputDTO
            {
                Id = medico.Usuario.Id,
                Tipo = medico.Usuario.Tipo
            };
        }

        return dto;
    }
}

public class MedicosComEspecialidadesDTO
{
    public List<MedicoOutputDTO> Medicos { get; set; } = new();
    public List<string> Especialidades { get; set; } = new();
}
