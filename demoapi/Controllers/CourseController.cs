namespace demoapi.Controllers;

using demoapi.Dtos;
using demoapi.Dtos.Courses;
using demoapi.Extensions;
using demoapi.Services;
using demoapi.Services.Courses;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CourseController : ControllerBase
{
    public const int MaxPageSize = 100;

    private readonly ICourseService _courses;

    public CourseController(ICourseService courses)
    {
        _courses = courses;
    }

    // GET /Course?search=cs&isActive=true&pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest($"Page must be >= 1 and pageSize must be between 1 and {MaxPageSize}.");

        var result = await _courses.ListAsync(search, isActive, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    // GET /Course/3
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCourse(int id, CancellationToken cancellationToken)
    {
        var result = await _courses.GetAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    // POST /Course
    [HttpPost]
    public async Task<IActionResult> CreateCourse(
        [FromBody] CourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _courses.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result, c => CreatedAtAction(nameof(GetCourse), new { id = c.Id }, c));
    }

    // PUT /Course/3
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(
        int id, [FromBody] CourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _courses.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    // DELETE /Course/3   (409 if the course has enrollments)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken cancellationToken)
    {
        var result = await _courses.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }
}