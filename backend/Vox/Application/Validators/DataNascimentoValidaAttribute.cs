namespace Vox.Application.Validators;

using System.ComponentModel.DataAnnotations;

public class DataNascimentoValidaAttribute : ValidationAttribute
{
    public int IdadeMaxima { get; set; } = 130;
    public int IdadeMinima { get; set; } = 4;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dataNascimento)
        {
            if (dataNascimento > DateTime.Today)
                return new ValidationResult(ErrorMessage ?? "A Data de Nascimento deve ser uma data passada.");

            var idade = DateTime.Today.Year - dataNascimento.Year;
            if (dataNascimento.Date > DateTime.Today.AddYears(-idade)) idade--;

            if (idade < IdadeMinima)
                return new ValidationResult(ErrorMessage ?? $"A idade mínima permitida é {IdadeMinima} anos.");

            if (idade > IdadeMaxima)
                return new ValidationResult(ErrorMessage ?? $"A idade máxima permitida é {IdadeMaxima} anos.");
        }
        else
        {
            return new ValidationResult(ErrorMessage ?? "O valor informado não é uma data válida.");
        }

        return ValidationResult.Success;
    }
}