namespace demoapi.Dtos.Students;

using System.ComponentModel.DataAnnotations;


[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class MinimumAgeAttribute : ValidationAttribute
{
    public int MinimumAge { get; }

    public MinimumAgeAttribute(int minimumAge)
    {
        MinimumAge = minimumAge;
        ErrorMessage = $"Age must be at least {minimumAge}.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not DateOnly dateOfBirth)
            return new ValidationResult("Enter a valid date of birth.");

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (dateOfBirth > today)
            return new ValidationResult("Date of birth cannot be in the future.");

        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
            age--;

        return age >= MinimumAge
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessageString);
    }
}