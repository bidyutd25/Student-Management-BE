using demoapi.Common.Results;
using demoapi.Data;
using demoapi.Data.Helpers;
using demoapi.Dtos.Scores;
using demoapi.Models;
using Microsoft.EntityFrameworkCore;

namespace demoapi.Services.Scores;

public class ScoreServiceImpl : IScoreService
{
    private readonly AppDbContext _context;

    public ScoreServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ScoreDto>> ListAsync(int enrollmentId, CancellationToken ct = default)
    {
        return await _context.Scores
            .AsNoTracking()
            .Where(s => s.EnrollmentId == enrollmentId)
            .OrderBy(s => s.AssessmentId)
            .Select(s => new ScoreDto(
                s.Id,
                s.EnrollmentId,
                s.AssessmentId,
                s.Assessment.Name,
                s.Assessment.MaxMarks,
                s.MarksObtained,
                s.Remarks))
            .ToListAsync(ct);
    }

    public async Task<ServiceResult<ScoreDto>> UpsertAsync(ScoreRequest request, CancellationToken ct = default)
    {
        var enrollment = await _context.Enrollments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EnrollmentId, ct);

        if (enrollment is null)
            return ServiceResult<ScoreDto>.NotFound("Enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Dropped)
            return ServiceResult<ScoreDto>.BadRequest("Scores cannot be recorded for a dropped enrollment.");

        var assessment = await _context.Assessments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AssessmentId, ct);

        if (assessment is null)
            return ServiceResult<ScoreDto>.NotFound("Assessment not found.");

        if (assessment.CourseId != enrollment.CourseId)
            return ServiceResult<ScoreDto>.BadRequest("This assessment does not belong to the enrolled course.");

        if (request.MarksObtained > assessment.MaxMarks)
            return ServiceResult<ScoreDto>.BadRequest($"Marks cannot be higher than the maximum of {assessment.MaxMarks}.");

        var score = await _context.Scores.FirstOrDefaultAsync(
            s => s.EnrollmentId == request.EnrollmentId && s.AssessmentId == request.AssessmentId, ct);

        if (score is null)
        {
            score = new ScoreEntity
            {
                EnrollmentId = request.EnrollmentId,
                AssessmentId = request.AssessmentId,
                CourseId = enrollment.CourseId,
                MarksObtained = request.MarksObtained,
                Remarks = request.Remarks?.Trim()
            };
            _context.Scores.Add(score);
        }
        else
        {
            score.MarksObtained = request.MarksObtained;
            score.Remarks = request.Remarks?.Trim();
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (DbErrors.IsRuleViolation(ex))
        {
            return ServiceResult<ScoreDto>.BadRequest(ex.InnerException!.Message);
        }
        catch (DbUpdateException ex) when (DbErrors.IsDuplicate(ex))
        {
            return ServiceResult<ScoreDto>.Conflict("A score already exists for this enrollment and assessment. Retry the request.");
        }

        return ServiceResult<ScoreDto>.Ok(new ScoreDto(
            score.Id,
            score.EnrollmentId,
            score.AssessmentId,
            assessment.Name,
            assessment.MaxMarks,
            score.MarksObtained,
            score.Remarks));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var score = await _context.Scores.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (score is null)
            return ServiceResult<bool>.NotFound("Score not found.");

        _context.Scores.Remove(score);
        await _context.SaveChangesAsync(ct);

        return ServiceResult<bool>.Ok(true);
    }
}