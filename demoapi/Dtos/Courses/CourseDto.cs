namespace demoapi.Dtos.Courses;

public sealed record CourseDto(
    int Id,
    string Code,
    string Title,
    byte Credits,
    bool IsActive);