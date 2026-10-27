using System.ComponentModel.DataAnnotations.Schema;

namespace demoapi.Models;

[Table("assessments")]
public class AssessmentEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("max_marks")]
    public decimal MaxMarks { get; set; }

    [Column("weightage")]
    public decimal Weightage { get; set; }

    public CourseEntity Course { get; set; } = null!;

    public List<ScoreEntity> Scores { get; set; } = new();
}