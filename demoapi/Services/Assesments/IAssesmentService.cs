using demoapi.Common.Results;
using demoapi.Dtos.Assessments;
using demoapi.Services;

namespace demoapi.Services.Assesments;

public interface IAssesmentService
{
    Task<List<AssessmentDto>> ListAsync(int? courseId, CancellationToken ct = default);

    Task<ServiceResult<AssessmentDto>> CreateAsync(AssessmentRequest request, CancellationToken ct = default);

    Task<ServiceResult<AssessmentDto>> UpdateAsync(int id, UpdateAssessmentRequest request, CancellationToken ct = default);

    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
}