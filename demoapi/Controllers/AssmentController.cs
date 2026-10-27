namespace demoapi.Controllers;

using demoapi.Dtos.Assessments;
using demoapi.Extensions;
using demoapi.Services.Assesments;
using demoapi.Services.Assessments;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AssessmentController : ControllerBase
{
    private readonly IAssesmentService _assessments;

    public AssessmentController(IAssesmentService assessments)
    {
        _assessments = assessments;
    }

    // GET /Assessment?courseId=3
    [HttpGet]
    public async Task<IActionResult> GetAssessments(
        [FromQuery] int? courseId = null, CancellationToken cancellationToken = default)
    {
        var result = await _assessments.ListAsync(courseId, cancellationToken);
        return Ok(result);
    }

    // POST /Assessment   { "courseId": 3, "name": "Midterm", "maxMarks": 50, "weightage": 30 }
    // 409 when the weightages of the course would add up to more than 100
    [HttpPost]
    public async Task<IActionResult> CreateAssessment(
        [FromBody] AssessmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _assessments.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result, a => StatusCode(StatusCodes.Status201Created, a));
    }

    // PUT /Assessment/4
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAssessment(
        int id, [FromBody] UpdateAssessmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _assessments.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    // DELETE /Assessment/4   (409 if scores exist)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAssessment(int id, CancellationToken cancellationToken)
    {
        var result = await _assessments.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }
}