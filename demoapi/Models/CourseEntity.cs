using System.ComponentModel.DataAnnotations.Schema;

namespace demoapi.Models;

[Table("courses")]
public class CourseEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("credits")]
    public byte Credits { get; set; } = 3;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    public List<AssessmentEntity> Assessments { get; set; } = new();

    public List<EnrollmentEntity> Enrollments { get; set; } = new();
}