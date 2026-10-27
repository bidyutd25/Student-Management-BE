using demoapi.Common.Results;
using demoapi.Data;
using demoapi.Data.Helpers;
using demoapi.Dtos.Assessments;
using demoapi.Models;
using demoapi.Services.Assesments;
using Microsoft.EntityFrameworkCore;

namespace demoapi.Services.Assessments;

    public class AssessmentServiceImpl : IAssesmentService
{
        private const decimal TotalWeightage = 100m;

        private readonly AppDbContext _context;

        public AssessmentServiceImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AssessmentDto>> ListAsync(int? courseId, CancellationToken ct = default)
        {
            IQueryable<AssessmentEntity> query = _context.Assessments.AsNoTracking();

            if (courseId.HasValue)
                query = query.Where(a => a.CourseId == courseId.Value);

            return await query
                .OrderBy(a => a.CourseId).ThenBy(a => a.Id)
                .Select(a => new AssessmentDto(a.Id, a.CourseId, a.Name, a.MaxMarks, a.Weightage))
                .ToListAsync(ct);
        }

        public async Task<ServiceResult<AssessmentDto>> CreateAsync(AssessmentRequest request, CancellationToken ct = default)
        {
            if (!await _context.Courses.AnyAsync(c => c.Id == request.CourseId, ct))
                return ServiceResult<AssessmentDto>.NotFound("Course not found.");

            var name = request.Name.Trim();

            if (await _context.Assessments.AnyAsync(a => a.CourseId == request.CourseId && a.Name == name, ct))
                return ServiceResult<AssessmentDto>.Conflict($"This course already has an assessment named '{name}'.");

            var used = await _context.Assessments
                .Where(a => a.CourseId == request.CourseId)
                .SumAsync(a => a.Weightage, ct);

            if (used + request.Weightage > TotalWeightage)
                return ServiceResult<AssessmentDto>.Conflict(
                    $"Total weightage would exceed 100 (already used: {used}, remaining: {TotalWeightage - used}).");

            var assessment = new AssessmentEntity
            {
                CourseId = request.CourseId,
                Name = name,
                MaxMarks = request.MaxMarks,
                Weightage = request.Weightage
            };

            _context.Assessments.Add(assessment);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (DbErrors.IsDuplicate(ex))
            {
                return ServiceResult<AssessmentDto>.Conflict($"This course already has an assessment named '{name}'.");
            }

            return ServiceResult<AssessmentDto>.Ok(ToDto(assessment));
        }

        public async Task<ServiceResult<AssessmentDto>> UpdateAsync(int id, UpdateAssessmentRequest request, CancellationToken ct = default)
        {
            var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (assessment is null)
                return ServiceResult<AssessmentDto>.NotFound("Assessment not found.");

            var name = request.Name.Trim();

            if (await _context.Assessments.AnyAsync(a => a.CourseId == assessment.CourseId && a.Name == name && a.Id != id, ct))
                return ServiceResult<AssessmentDto>.Conflict($"This course already has an assessment named '{name}'.");

            var usedByOthers = await _context.Assessments
                .Where(a => a.CourseId == assessment.CourseId && a.Id != id)
                .SumAsync(a => a.Weightage, ct);

            if (usedByOthers + request.Weightage > TotalWeightage)
                return ServiceResult<AssessmentDto>.Conflict(
                    $"Total weightage would exceed 100 (used by other assessments: {usedByOthers}).");

            // Lowering max marks must not invalidate scores that were already recorded
            if (await _context.Scores.AnyAsync(s => s.AssessmentId == id && s.MarksObtained > request.MaxMarks, ct))
                return ServiceResult<AssessmentDto>.Conflict(
                    "Existing scores are higher than the new max marks. Correct those scores first.");

            assessment.Name = name;
            assessment.MaxMarks = request.MaxMarks;
            assessment.Weightage = request.Weightage;

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (DbErrors.IsDuplicate(ex))
            {
                return ServiceResult<AssessmentDto>.Conflict($"This course already has an assessment named '{name}'.");
            }

            return ServiceResult<AssessmentDto>.Ok(ToDto(assessment));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (assessment is null)
                return ServiceResult<bool>.NotFound("Assessment not found.");

            if (await _context.Scores.AnyAsync(s => s.AssessmentId == id, ct))
                return ServiceResult<bool>.Conflict("This assessment already has scores. Delete those scores first.");

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        private static AssessmentDto ToDto(AssessmentEntity a) =>
            new(a.Id, a.CourseId, a.Name, a.MaxMarks, a.Weightage);
    }