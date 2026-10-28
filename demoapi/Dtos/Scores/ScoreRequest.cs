namespace demoapi.Dtos.Scores;

using System.ComponentModel.DataAnnotations;

public sealed class ScoreRequest
{
    [Range(1, int.MaxValue)]
    public int EnrollmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int AssessmentId { get; set; }

    [Range(0, 100, ErrorMessage = "Marks must be between 0 and 100.")]
    public decimal MarksObtained { get; set; }

    [StringLength(255)]
    public string? Remarks { get; set; }
}