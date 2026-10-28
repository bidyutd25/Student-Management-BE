using demoapi.Common.Results;
using demoapi.Data;
using demoapi.Data.Helpers;
using demoapi.Dtos.Courses;
using demoapi.Models;
using Microsoft.EntityFrameworkCore;

namespace demoapi.Services.Courses;

public class CourseServiceImpl : ICourseService
{
    private readonly AppDbContext _context;

    public CourseServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<CourseDto>> ListAsync(
        string? search, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        IQueryable<CourseEntity> query = _context.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Code.Contains(search) || c.Title.Contains(search));

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new CourseDto(c.Id, c.Code, c.Title, c.Credits, c.IsActive))
            .ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<ServiceResult<CourseDto>> GetAsync(int id, CancellationToken ct = default)
    {
        var dto = await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseDto(c.Id, c.Code, c.Title, c.Credits, c.IsActive))
            .FirstOrDefaultAsync(ct);

        return dto is null
            ? ServiceResult<CourseDto>.NotFound("Course not found.")
            : ServiceResult<CourseDto>.Ok(dto);
    }

    public async Task<ServiceResult<CourseDto>> CreateAsync(CourseRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        if (await _context.Courses.AnyAsync(c => c.Code == code, ct))
            return ServiceResult<CourseDto>.Conflict($"Course code '{code}' already exists.");

        var course = new CourseEntity
        {
            Code = code,
            Title = request.Title.Trim(),
            Credits = request.Credits,
            IsActive = request.IsActive
        };

        if (CodeEqualsTitle(code, request.Title))
            return ServiceResult<CourseDto>.BadRequest(CodeEqualsTitleMessage);

        _context.Courses.Add(course);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (DbErrors.IsDuplicate(ex))
        {
            return ServiceResult<CourseDto>.Conflict($"Course code '{code}' already exists.");
        }

        return ServiceResult<CourseDto>.Ok(ToDto(course));
    }

    public async Task<ServiceResult<CourseDto>> UpdateAsync(int id, CourseRequest request, CancellationToken ct = default)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course is null)
            return ServiceResult<CourseDto>.NotFound("Course not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (CodeEqualsTitle(code, request.Title))
            return ServiceResult<CourseDto>.BadRequest(CodeEqualsTitleMessage);
        if (await _context.Courses.AnyAsync(c => c.Code == code && c.Id != id, ct))
            return ServiceResult<CourseDto>.Conflict($"Course code '{code}' already exists.");

        course.Code = code;
        course.Title = request.Title.Trim();
        course.Credits = request.Credits;
        course.IsActive = request.IsActive;

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (DbErrors.IsDuplicate(ex))
        {
            return ServiceResult<CourseDto>.Conflict($"Course code '{code}' already exists.");
        }

        return ServiceResult<CourseDto>.Ok(ToDto(course));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course is null)
            return ServiceResult<bool>.NotFound("Course not found.");

        if (await _context.Enrollments.AnyAsync(e => e.CourseId == id, ct))
            return ServiceResult<bool>.Conflict("This course has enrollments. Set IsActive to false instead of deleting it.");

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(ct);

        return ServiceResult<bool>.Ok(true);
    }

    private const string CodeEqualsTitleMessage = "Course code and title cannot be the same.";

    private static bool CodeEqualsTitle(string code, string title) =>
        string.Equals(code.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase);

    private static CourseDto ToDto(CourseEntity c) => new(c.Id, c.Code, c.Title, c.Credits, c.IsActive);
}