using System.ComponentModel.DataAnnotations;
using demoapi.Common.Validation;

namespace demoapi.Dtos.Students;

public sealed class UpdateStudentRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [RegularExpression(
        ValidationPatterns.Email,
        ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }
}