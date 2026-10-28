using demoapi.Common.Results;
using demoapi.Data;
using demoapi.Dtos.Scorecards;
using demoapi.Models;
using Microsoft.EntityFrameworkCore;

namespace demoapi.Services.Scorecards;

    public class ScorecardServiceImpl : IScorecardService
    {
        private const string Pending = "PENDING";

        private readonly AppDbContext _context;

        public ScorecardServiceImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<StudentScorecardDto>> GetForStudentAsync(int studentId, CancellationToken ct = default)
        {
            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentId, ct);

            if (student is null)
                return ServiceResult<StudentScorecardDto>.NotFound("Student not found.");

            var enrollments = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId && e.Status != EnrollmentStatus.Dropped)
                .Include(e => e.Course).ThenInclude(c => c.Assessments)
                .Include(e => e.Scores)
                .AsSplitQuery()
                .OrderBy(e => e.Course.Code)
                .ToListAsync(ct);

            var courses = enrollments.Select(BuildCourseResult).ToList();

            // The overall figure only counts courses where every assessment has been scored
            var finished = courses.Where(c => c.IsComplete).ToList();
            var overall = ScorecardMath.Overall(enrollments);

        return ServiceResult<StudentScorecardDto>.Ok(
                new StudentScorecardDto(student.Id, student.Name, courses, overall));
        }

        public async Task<ServiceResult<EnrollmentScorecardDto>> GetForEnrollmentAsync(int enrollmentId, CancellationToken ct = default)
        {
            var enrollment = await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Course).ThenInclude(c => c.Assessments)
                .Include(e => e.Scores)
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);

            if (enrollment is null)
                return ServiceResult<EnrollmentScorecardDto>.NotFound("Enrollment not found.");

            return ServiceResult<EnrollmentScorecardDto>.Ok(new EnrollmentScorecardDto(
                enrollment.StudentId,
                enrollment.Student.Name,
                BuildCourseResult(enrollment)));
        }

        private static CourseResultDto BuildCourseResult(EnrollmentEntity enrollment)
        {
            var assessments = enrollment.Course.Assessments.OrderBy(a => a.Id).ToList();

            decimal finalPercentage = 0m;
            var isComplete = false;
            var rows = new List<AssessmentResultDto>();

            foreach (var assessment in assessments)
            {
                var score = enrollment.Scores.FirstOrDefault(s => s.AssessmentId == assessment.Id);

                decimal? weighted = null;
                if (score is not null)
                {
                    var raw = score.MarksObtained / assessment.MaxMarks * assessment.Weightage;
                    finalPercentage += raw;
                    weighted = Math.Round(raw, 2);
                }

                rows.Add(new AssessmentResultDto(
                    assessment.Id,
                    assessment.Name,
                    assessment.MaxMarks,
                    assessment.Weightage,
                    score?.MarksObtained,
                    weighted));
            }

            (finalPercentage, isComplete) = ScorecardMath.Compute(enrollment);
            var grade = isComplete ? GradeCalculator.FromPercentage(finalPercentage) : Pending;

            return new CourseResultDto(
                enrollment.Id,
                enrollment.CourseId,
                enrollment.Course.Code,
                enrollment.Course.Title,
                enrollment.Status,
                rows,
                finalPercentage,
                grade,
                isComplete);
        }
    }