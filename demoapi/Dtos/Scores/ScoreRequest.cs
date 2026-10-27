namespace demoapi.Dtos.Scores;

using System.ComponentModel.DataAnnotations;

public sealed class ScoreRequest
{
    [Range(1, int.MaxValue)]
    public int EnrollmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int AssessmentId { get; set; }

    [Range(typeof(decimal), "0", "10000",
        ParseLimitsInInvariantCulture = true)]
    public decimal MarksObtained { get; set; }

    [StringLength(255)]
    public string? Remarks { get; set; }
}