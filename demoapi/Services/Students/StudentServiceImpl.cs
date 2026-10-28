using demoapi.Common.Results;
using demoapi.Data;
using demoapi.Data.Helpers;
using demoapi.Dtos.Students;
using demoapi.Models;
using demoapi.Services.Scorecards;
using Microsoft.EntityFrameworkCore;

namespace demoapi.Services.Students;

public class StudentServiceImpl : IStudentService
{
    private readonly AppDbContext _context;

    public StudentServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<StudentDto>> ListAsync(
        string? name, string? email, string sortBy, string sortOrder,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        IQueryable<StudentEntity> query = _context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(s => s.Email.Contains(email));

        var descending = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        // Id is the tie-breaker so paging stays stable
        query = sortBy.ToLowerInvariant() switch
        {
            "name" => descending
                ? query.OrderByDescending(s => s.Name).ThenByDescending(s => s.Id)
                : query.OrderBy(s => s.Name).ThenBy(s => s.Id),
            "email" => descending
                ? query.OrderByDescending(s => s.Email).ThenByDescending(s => s.Id)
                : query.OrderBy(s => s.Email).ThenBy(s => s.Id),
            "dateofbirth" => descending
                ? query.OrderByDescending(s => s.DateOfBirth).ThenByDescending(s => s.Id)
                : query.OrderBy(s => s.DateOfBirth).ThenBy(s => s.Id),
            _ => descending
                ? query.OrderByDescending(s => s.Id)
                : query.OrderBy(s => s.Id)
        };

        var page = await query
            .Select(s => new StudentDto(
                s.Id,
                s.Name,
                s.Email,
                s.DateOfBirth,
                s.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)))
            .ToPagedResponseAsync(pageNumber, pageSize, ct);
        var overall = await LoadOverallScoresAsync(page.Data.Select(s => s.Id).ToList(), ct);
        return new PagedResponse<StudentDto>
        {
            Data = page.Data
                .Select(s => s with { OverallPercentage = overall.GetValueOrDefault(s.Id) })
                .ToList(),
            Pagination = page.Pagination
        };
    }

    private async Task<Dictionary<int, decimal?>> LoadOverallScoresAsync(
        List<int> studentIds, CancellationToken ct)
    {
        if (studentIds.Count == 0)
            return new Dictionary<int, decimal?>();

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => studentIds.Contains(e.StudentId) && e.Status != EnrollmentStatus.Dropped)
            .Include(e => e.Course).ThenInclude(c => c.Assessments)
            .Include(e => e.Scores)
            .AsSplitQuery()
            .ToListAsync(ct);

        return enrollments
            .GroupBy(e => e.StudentId)
            .ToDictionary(g => g.Key, g => ScorecardMath.Overall(g));
    }

    public async Task<ServiceResult<StudentDto>> GetAsync(int id, CancellationToken ct = default)
    {
        var dto = await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentDto(
                s.Id,
                s.Name,
                s.Email,
                s.DateOfBirth,
                s.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)))
            .FirstOrDefaultAsync(ct);

        return dto is null
            ? ServiceResult<StudentDto>.NotFound("Student not found.")
            : ServiceResult<StudentDto>.Ok(dto);
    }

    public async Task<ServiceResult<StudentDto>> CreateAsync(CreateStudentRequest request, CancellationToken ct = default)
    {
        var courseIds = request.CourseIds.Distinct().ToList();
        if (courseIds.Count != request.CourseIds.Count)
            return ServiceResult<StudentDto>.BadRequest("Duplicate course ids are not allowed.");

        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => courseIds.Contains(c.Id))
            .ToListAsync(ct);

        if (courses.Count != courseIds.Count)
            return ServiceResult<StudentDto>.BadRequest("One or more courses do not exist.");

        var closed = courses.FirstOrDefault(c => !c.IsActive);
        if (closed is not null)
            return ServiceResult<StudentDto>.BadRequest($"Course {closed.Code} is not open for enrollment.");

        var email = request.Email.Trim();
        if (await _context.Students.AnyAsync(s => s.Email == email, ct))
            return ServiceResult<StudentDto>.Conflict("A student with this email already exists.");

        var student = new StudentEntity
        {
            Name = request.Name.Trim(),
            Email = email,
            DateOfBirth = request.DateOfBirth
        };

        var today = DateOnly.FromDateTime(DateTime.Today);
        foreach (var courseId in courseIds)
        {
            student.Enrollments.Add(new EnrollmentEntity
            {
                CourseId = courseId,
                Status = EnrollmentStatus.Active,
                EnrolledOn = today
            });
        }

        // One SaveChanges = one transaction: the student and the 1-3 enrollments are saved together
        _context.Students.Add(student);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (DbErrors.IsRuleViolation(ex))
        {
            return ServiceResult<StudentDto>.Conflict(ex.InnerException!.Message);
        }
        catch (DbUpdateException ex) when (DbErrors.IsDuplicate(ex))
        {
            return ServiceResult<StudentDto>.Conflict("A student with this email already exists.");
        }

        return ServiceResult<StudentDto>.Ok(
            new StudentDto(student.Id, student.Name, student.Email, student.DateOfBirth, courseIds.Count));
    }

    public async Task<ServiceResult<StudentDto>> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct = default)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null)
            return ServiceResult<StudentDto>.NotFound("Student not found.");

        var email = request.Email.Trim();
        if (await _context.Students.AnyAsync(s => s.Email == email && s.Id != id, ct))
            return ServiceResult<StudentDto>.Conflict("Another student already uses this email.");

        student.Name = request.Name.Trim();
        student.Email = email;
        student.DateOfBirth = request.DateOfBirth;

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (DbErrors.IsDuplicate(ex))
        {
            return ServiceResult<StudentDto>.Conflict("Another student already uses this email.");
        }

        return await GetAsync(id, ct);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null)
            return ServiceResult<bool>.NotFound("Student not found.");

        // Enrollments and scores are removed by the foreign keys (ON DELETE CASCADE)
        _context.Students.Remove(student);
        await _context.SaveChangesAsync(ct);

        return ServiceResult<bool>.Ok(true);
    }
}