using demoapi.Common.Results;
using demoapi.Dtos.Enrollments;
using demoapi.Models;

namespace demoapi.Services.Enrollments;

public interface IEnrollmentService
{
    Task<PagedResponse<EnrollmentDto>> ListAsync(
        int? studentId, int? courseId, EnrollmentStatus? status,
        int pageNumber, int pageSize, CancellationToken ct = default);

    Task<ServiceResult<EnrollmentDto>> EnrollAsync(EnrollRequest request, CancellationToken ct = default);

    Task<ServiceResult<EnrollmentDto>> ChangeStatusAsync(int id, EnrollmentStatus status, CancellationToken ct = default);
}