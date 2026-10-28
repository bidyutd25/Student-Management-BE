namespace demoapi.Controllers;

using demoapi.Dtos;
using demoapi.Dtos.Students;
using demoapi.Extensions;
using demoapi.Services.Students;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class StudentController : ControllerBase
{
    private static readonly string[] AllowedSortColumns = { "Id", "Name", "Email", "DateOfBirth" };

    public const int MaxPageSize = 100;

    private readonly IStudentService _students;

    public StudentController(IStudentService students)
    {
        _students = students;
    }

    // GET /Student?name=&email=&pageNumber=1&pageSize=20&sortBy=Id&sortOrder=asc
    [HttpGet]
    public async Task<IActionResult> GetStudents(
        [FromQuery] string? name = null,
        [FromQuery] string? email = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "Id",
        [FromQuery] string sortOrder = "asc",
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest($"Page must be >= 1 and pageSize must be between 1 and {MaxPageSize}.");

        if (!AllowedSortColumns.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Invalid sortBy. Use Id, Name, Email, or DateOfBirth.");

        if (sortOrder.ToLowerInvariant() is not ("asc" or "desc"))
            return BadRequest("Invalid sortOrder. Use asc or desc.");

        var result = await _students.ListAsync(
            name, email, sortBy, sortOrder, pageNumber, pageSize, cancellationToken);

        return Ok(result);
    }

    // GET /Student/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStudent(int id, CancellationToken cancellationToken)
    {
        var result = await _students.GetAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    // POST /Student   (a student starts with 1 to 3 courses)
    [HttpPost]
    public async Task<IActionResult> CreateStudent(
        [FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _students.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result, s => CreatedAtAction(nameof(GetStudent), new { id = s.Id }, s));
    }

    // PUT /Student/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStudent(
        int id, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _students.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    // DELETE /Student/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStudent(int id, CancellationToken cancellationToken)
    {
        var result = await _students.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }
}