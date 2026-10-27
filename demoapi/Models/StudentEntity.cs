using System.ComponentModel.DataAnnotations.Schema;

namespace demoapi.Models;

[Table("students")]
public class StudentEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("email")]
    public string Email { get; set; } = "";

    [Column("date_of_birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Column("created_at"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

    public List<EnrollmentEntity> Enrollments { get; set; } = new();
}