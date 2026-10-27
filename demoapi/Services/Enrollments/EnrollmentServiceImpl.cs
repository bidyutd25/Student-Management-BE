using demoapi.Common.Results;
using demoapi.Data;
using demoapi.Data.Helpers;
using demoapi.Dtos.Enrollments;
using demoapi.Models;
using Microsoft.EntityFrameworkCore;

namespace demoapi.Services.Enrollments;


    public class EnrollmentServiceImpl : IEnrollmentService
    {
        public const int MaxActiveCourses = 3;

        private readonly AppDbContext _context;

        public EnrollmentServiceImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<EnrollmentDto>> ListAsync(
            int? studentId, int? courseId, EnrollmentStatus? status,
            int pageNumber, int pageSize, CancellationToken ct = default)
        {
            IQueryable<EnrollmentEntity> query = _context.Enrollments.AsNoTracking();

            if (studentId.HasValue)
                query = query.Where(e => e.StudentId == studentId.Value);

            if (courseId.HasValue)
                query = query.Where(e => e.CourseId == courseId.Value);

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            return await Project(query.OrderBy(e => e.Id))
                .ToPagedResponseAsync(pageNumber, pageSize, ct);
        }

        public async Task<ServiceResult<EnrollmentDto>> EnrollAsync(EnrollRequest request, CancellationToken ct = default)
        {
            if (!await _context.Students.AnyAsync(s => s.Id == request.StudentId, ct))
                return ServiceResult<EnrollmentDto>.NotFound("Student not found.");

            var course = await _context.Courses.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CourseId, ct);

            if (course is null)
                return ServiceResult<EnrollmentDto>.NotFound("Course not found.");

            if (!course.IsActive)
                return ServiceResult<EnrollmentDto>.BadRequest($"Course {course.Code} is not open for enrollment.");

            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == request.StudentId && e.CourseId == request.CourseId, ct);

            if (existing is { Status: EnrollmentStatus.Active })
                return ServiceResult<EnrollmentDto>.Conflict("The student is already enrolled in this course.");

            if (existing is { Status: EnrollmentStatus.Completed })
                return ServiceResult<EnrollmentDto>.Conflict("The student has already completed this course.");

            var activeCount = await CountActiveAsync(request.StudentId, ct);
            if (activeCount >= MaxActiveCourses)
                return ServiceResult<EnrollmentDto>.Conflict($"A student can have at most {MaxActiveCourses} active courses.");

            var today = DateOnly.FromDateTime(DateTime.Today);
            EnrollmentEntity enrollment;

            if (existing is null)
            {
                enrollment = new EnrollmentEntity
                {
                    StudentId = request.StudentId,
                    CourseId = request.CourseId,
                    Status = EnrollmentStatus.Active,
                    EnrolledOn = today
                };
                _context.Enrollments.Add(enrollment);
            }
            else
            {
                // Re-enrolling after a drop: the (student, course) pair is unique, so reuse the row
                enrollment = existing;
                enrollment.Status = EnrollmentStatus.Active;
                enrollment.EnrolledOn = today;
            }

            var saved = await TrySaveAsync(ct);
            if (saved is not null)
                return ServiceResult<EnrollmentDto>.Conflict(saved);

            return ServiceResult<EnrollmentDto>.Ok(await LoadDtoAsync(enrollment.Id, ct));
        }

        public async Task<ServiceResult<EnrollmentDto>> ChangeStatusAsync(int id, EnrollmentStatus status, CancellationToken ct = default)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (enrollment is null)
                return ServiceResult<EnrollmentDto>.NotFound("Enrollment not found.");

            if (enrollment.Status != status)
            {
                var activeCount = await CountActiveAsync(enrollment.StudentId, ct);

                switch (enrollment.Status, status)
                {
                    case (EnrollmentStatus.Active, EnrollmentStatus.Dropped):
                        // A student must always keep at least one active course
                        if (activeCount <= 1)
                            return ServiceResult<EnrollmentDto>.Conflict(
                                "A student must keep at least one active course. Enroll in another course before dropping this one.");
                        break;

                    case (EnrollmentStatus.Active, EnrollmentStatus.Completed):
                        break;

                    case (EnrollmentStatus.Dropped, EnrollmentStatus.Active):
                        if (activeCount >= MaxActiveCourses)
                            return ServiceResult<EnrollmentDto>.Conflict(
                                $"A student can have at most {MaxActiveCourses} active courses.");
                        break;

                    default:
                        return ServiceResult<EnrollmentDto>.Conflict(
                            $"Cannot change status from {enrollment.Status} to {status}.");
                }

                enrollment.Status = status;

                var saved = await TrySaveAsync(ct);
                if (saved is not null)
                    return ServiceResult<EnrollmentDto>.Conflict(saved);
            }

            return ServiceResult<EnrollmentDto>.Ok(await LoadDtoAsync(enrollment.Id, ct));
        }

        // ---------------------------------------------------------------

        private Task<int> CountActiveAsync(int studentId, CancellationToken ct) =>
            _context.Enrollments.CountAsync(
                e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active, ct);

        private static IQueryable<EnrollmentDto> Project(IQueryable<EnrollmentEntity> query) =>
            query.Select(e => new EnrollmentDto(
                e.Id,
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Code,
                e.Course.Title,
                e.Status,
                e.EnrolledOn));

        private Task<EnrollmentDto> LoadDtoAsync(int id, CancellationToken ct) =>
            Project(_context.Enrollments.AsNoTracking().Where(e => e.Id == id)).FirstAsync(ct);

        /// <returns>null on success, otherwise the message to return to the caller.</returns>
        private async Task<string?> TrySaveAsync(CancellationToken ct)
        {
            try
            {
                await _context.SaveChangesAsync(ct);
                return null;
            }
            catch (DbUpdateException ex) when (DbErrors.IsRuleViolation(ex))
            {
                // The database trigger is the final guard (for example two requests at the same moment)
                return ex.InnerException!.Message;
            }
            catch (DbUpdateException ex) when (DbErrors.IsDuplicate(ex))
            {
                return "The student is already enrolled in this course.";
            }
        }
    }