namespace demoapi.Dtos.Enrollments;
using demoapi.Models;

public sealed record EnrollmentDto(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    EnrollmentStatus Status,
    DateOnly EnrolledOn);