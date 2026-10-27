namespace demoapi.Controllers;

using demoapi.Extensions;
using demoapi.Services;
using demoapi.Services.Scorecards;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ScorecardController : ControllerBase
{
    private readonly IScorecardService _scorecards;

    public ScorecardController(IScorecardService scorecards)
    {
        _scorecards = scorecards;
    }

    // GET /Scorecard/student/1
    // Every non-dropped course of the student with final percentage, grade and overall percentage
    [HttpGet("student/{studentId:int}")]
    public async Task<IActionResult> GetStudentScorecard(int studentId, CancellationToken cancellationToken)
    {
        var result = await _scorecards.GetForStudentAsync(studentId, cancellationToken);
        return this.ToActionResult(result);
    }

    // GET /Scorecard/enrollment/7
    // One course with the breakdown per assessment
    [HttpGet("enrollment/{enrollmentId:int}")]
    public async Task<IActionResult> GetEnrollmentScorecard(int enrollmentId, CancellationToken cancellationToken)
    {
        var result = await _scorecards.GetForEnrollmentAsync(enrollmentId, cancellationToken);
        return this.ToActionResult(result);
    }
}