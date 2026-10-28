namespace demoapi.Services.Scorecards;

using demoapi.Models;

public static class ScorecardMath
{
    public static (decimal FinalPercentage, bool IsComplete) Compute(EnrollmentEntity enrollment)
    {
        var assessments = enrollment.Course.Assessments;

        decimal total = 0m;
        var scored = 0;

        foreach (var assessment in assessments)
        {
            var score = enrollment.Scores.FirstOrDefault(s => s.AssessmentId == assessment.Id);
            if (score is null)
                continue;

            scored++;
            total += score.MarksObtained / assessment.MaxMarks * assessment.Weightage;
        }

        var isComplete = assessments.Count > 0 && scored == assessments.Count;
        return (Math.Round(total, 2), isComplete);
    }

    public static decimal? Overall(IEnumerable<EnrollmentEntity> enrollments)
    {
        var finished = enrollments
            .Select(Compute)
            .Where(result => result.IsComplete)
            .Select(result => result.FinalPercentage)
            .ToList();

        return finished.Count == 0 ? null : Math.Round(finished.Average(), 2);
    }
}