using System.ComponentModel.DataAnnotations;
using demoapi.Common.Validation;

namespace demoapi.Dtos.Students;

public sealed class CreateStudentRequest
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

    [Required]
    [MinLength(1, ErrorMessage = "Select at least 1 course.")]
    [MaxLength(3, ErrorMessage = "Select at most 3 courses.")]
    public List<int> CourseIds { get; set; } = new();
}