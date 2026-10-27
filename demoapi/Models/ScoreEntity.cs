using System.ComponentModel.DataAnnotations.Schema;

namespace demoapi.Models;

[Table("scores")]
public class ScoreEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    [Column("assessment_id")]
    public int AssessmentId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("marks_obtained")]
    public decimal MarksObtained { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("recorded_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime RecordedAt { get; set; }

    public EnrollmentEntity Enrollment { get; set; } = null!;

    public AssessmentEntity Assessment { get; set; } = null!;
}