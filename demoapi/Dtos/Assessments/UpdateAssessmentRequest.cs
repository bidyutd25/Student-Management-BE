namespace demoapi.Dtos.Assessments;
using System.ComponentModel.DataAnnotations;

public sealed class UpdateAssessmentRequest
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "10000",
        ParseLimitsInInvariantCulture = true)]
    public decimal MaxMarks { get; set; }

    [Range(typeof(decimal), "0.01", "100",
        ParseLimitsInInvariantCulture = true)]
    public decimal Weightage { get; set; }
}