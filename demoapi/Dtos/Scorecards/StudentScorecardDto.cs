namespace demoapi.Dtos.Scorecards;

public sealed record StudentScorecardDto(
    int StudentId,
    string StudentName,
    List<CourseResultDto> Courses,
    decimal? OverallPercentage);