namespace demoapi.Dtos.Scorecards;
public sealed record EnrollmentScorecardDto(
    int StudentId,
    string StudentName,
    CourseResultDto Course);