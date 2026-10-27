namespace demoapi.Controllers;

using demoapi.Dtos;
using demoapi.Dtos.Scores;
using demoapi.Extensions;
using demoapi.Services.Scores;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ScoreController : ControllerBase
{
    private readonly IScoreService _scores;

    public ScoreController(IScoreService scores)
    {
        _scores = scores;
    }

    // GET /Score?enrollmentId=7
    [HttpGet]
    public async Task<IActionResult> GetScores(
        [FromQuery] int enrollmentId, CancellationToken cancellationToken)
    {
        if (enrollmentId < 1)
            return BadRequest("enrollmentId is required.");

        var result = await _scores.ListAsync(enrollmentId, cancellationToken);
        return Ok(result);
    }

    // PUT /Score   { "enrollmentId": 7, "assessmentId": 4, "marksObtained": 42, "remarks": "" }
    // Creates the score, or replaces the marks if one already exists for that assessment
    [HttpPut]
    public async Task<IActionResult> SaveScore(
        [FromBody] ScoreRequest request, CancellationToken cancellationToken)
    {
        var result = await _scores.UpsertAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    // DELETE /Score/12
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteScore(int id, CancellationToken cancellationToken)
    {
        var result = await _scores.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }
}