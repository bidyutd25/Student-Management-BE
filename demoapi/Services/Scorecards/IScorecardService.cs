using demoapi.Common.Results;
using demoapi.Dtos.Scorecards;

namespace demoapi.Services.Scorecards;
public interface IScorecardService
{
    /// <summary>All non-dropped courses of a student, with the final percentage and grade of each.</summary>
    Task<ServiceResult<StudentScorecardDto>> GetForStudentAsync(int studentId, CancellationToken ct = default);

    /// <summary>One enrolled course with its assessment-by-assessment breakdown.</summary>
    Task<ServiceResult<EnrollmentScorecardDto>> GetForEnrollmentAsync(int enrollmentId, CancellationToken ct = default);
}