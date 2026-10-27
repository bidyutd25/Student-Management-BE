namespace demoapi.Dtos.Scorecards;

public sealed record AssessmentResultDto(
    int AssessmentId,
    string Name,
    decimal MaxMarks,
    decimal Weightage,
    decimal? MarksObtained,
    decimal? WeightedScore);