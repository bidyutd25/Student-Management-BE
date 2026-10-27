namespace demoapi.Controllers;

using demoapi.Dtos.Enrollments;
using demoapi.Extensions;
using demoapi.Models;
using demoapi.Services.Enrollments;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class EnrollmentController : ControllerBase
{
    public const int MaxPageSize = 100;

    private readonly IEnrollmentService _enrollments;

    public EnrollmentController(IEnrollmentService enrollments)
    {
        _enrollments = enrollments;
    }

    // GET /Enrollment?studentId=1&courseId=&status=Active&pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetEnrollments(
        [FromQuery] int? studentId = null,
        [FromQuery] int? courseId = null,
        [FromQuery] EnrollmentStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest($"Page must be >= 1 and pageSize must be between 1 and {MaxPageSize}.");

        var result = await _enrollments.ListAsync(
            studentId, courseId, status, pageNumber, pageSize, cancellationToken);

        return Ok(result);
    }

    // POST /Enrollment   { "studentId": 1, "courseId": 2 }
    // 409 when the student already has 3 active courses
    [HttpPost]
    public async Task<IActionResult> Enroll(
        [FromBody] EnrollRequest request, CancellationToken cancellationToken)
    {
        var result = await _enrollments.EnrollAsync(request, cancellationToken);
        return this.ToActionResult(result, e => StatusCode(StatusCodes.Status201Created, e));
    }

    // PUT /Enrollment/7/status   { "status": "Dropped" }   (Active, Completed or Dropped)
    // 409 when dropping the student's last active course
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id, [FromBody] UpdateEnrollmentStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _enrollments.ChangeStatusAsync(id, request.Status, cancellationToken);
        return this.ToActionResult(result);
    }
}