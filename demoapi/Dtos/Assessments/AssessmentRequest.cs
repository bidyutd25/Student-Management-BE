namespace demoapi.Dtos.Assessments;
using System.ComponentModel.DataAnnotations;

public sealed class AssessmentRequest
{
    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Assessment max marks must be between 0 and 100.")]
    public decimal MaxMarks { get; set; }

    [Range(0, 100, ErrorMessage = "Weightage must be between 0 and 100.")]
    public decimal Weightage { get; set; }
}