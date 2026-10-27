namespace demoapi.Dtos.Assessments;

public sealed record AssessmentDto(
    int Id,
    int CourseId,
    string Name,
    decimal MaxMarks,
    decimal Weightage);