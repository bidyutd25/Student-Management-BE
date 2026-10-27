using System.ComponentModel.DataAnnotations.Schema;

namespace demoapi.Models;

[Table("enrollments")]
public class EnrollmentEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("status")]
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    [Column("enrolled_on")]
    public DateOnly EnrolledOn { get; set; }

    public StudentEntity Student { get; set; } = null!;

    public CourseEntity Course { get; set; } = null!;

    public List<ScoreEntity> Scores { get; set; } = new();
}