namespace Vox.Application.Validators;
    
using System.ComponentModel.DataAnnotations;

using Vox.Domain.Enums;

public class SexoValidoAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        if (!Enum.IsDefined(typeof(PessoaEnum), value))
        {
            return new ValidationResult(ErrorMessage ?? "O valor de sexo é inválido.");
        }

        return ValidationResult.Success;
    }
}
