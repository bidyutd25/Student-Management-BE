namespace demoapi.Dtos.Enrollments;
using System.ComponentModel.DataAnnotations;


public sealed class EnrollRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }
}