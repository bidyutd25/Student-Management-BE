using System.ComponentModel.DataAnnotations;
using demoapi.Common.Validation;

namespace demoapi.Dtos.Students;

public sealed class CreateStudentRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(StudentRules.NameMaxLength, MinimumLength = StudentRules.NameMinLength,
    ErrorMessage = "Name must be at least 3 characters long.")]
    [RegularExpression(StudentRules.NamePattern,
    ErrorMessage = "Name can contain only letters and single spaces, with at least 3 letters.")]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(150)]
    [RegularExpression(
        ValidationPatterns.Email,
        ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required.")]
    [MinimumAge(StudentRules.MinimumAge, ErrorMessage = "Student age must be greater than 15.")]
    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "A student must be enrolled in at least 1 course.")]
    [MinLength(1, ErrorMessage = "A student must be enrolled in at least 1 course.")]
    [MaxLength(3, ErrorMessage = "A student can be enrolled in at most 3 courses.")]
    public List<int> CourseIds { get; set; } = new();
}