namespace demoapi.Dtos.Courses;
using System.ComponentModel.DataAnnotations;


public sealed class CourseRequest
{
    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 10)]
    public byte Credits { get; set; } = 3;

    public bool IsActive { get; set; } = true;
}