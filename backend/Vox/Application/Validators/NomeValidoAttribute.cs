namespace Vox.Application.Validators;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class NomeValidoAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        var nome = value.ToString()!;
        if (!Regex.IsMatch(nome, @"^[A-Za-zÀ-ÿ\s]+$"))
        {
            return new ValidationResult(ErrorMessage ?? "O nome contém caracteres inválidos.");
        }

        return ValidationResult.Success;
    }
}
