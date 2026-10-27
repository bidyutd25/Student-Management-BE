namespace demoapi.Dtos.Scores;

public sealed record ScoreDto(
    int Id,
    int EnrollmentId,
    int AssessmentId,
    string AssessmentName,
    decimal MaxMarks,
    decimal MarksObtained,
    string? Remarks);