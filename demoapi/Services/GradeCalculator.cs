namespace demoapi.Services;

public static class GradeCalculator
{
    // Keep these bands consistent with the v_scorecard view.
    public static string FromPercentage(decimal percentage) =>
        percentage switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B",
            >= 60 => "C",
            >= 50 => "D",
            _ => "F"
        };
}