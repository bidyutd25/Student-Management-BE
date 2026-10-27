namespace demoapi.Dtos.Scorecards;
using demoapi.Models;

public sealed record CourseResultDto(
    int EnrollmentId,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    EnrollmentStatus Status,
    List<AssessmentResultDto> Assessments,
    decimal FinalPercentage,
    string Grade,
    bool IsComplete);