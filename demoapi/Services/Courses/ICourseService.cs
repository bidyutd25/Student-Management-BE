using demoapi.Common.Results;
using demoapi.Dtos.Courses;
using demoapi.Models;

namespace demoapi.Services.Courses;
public interface ICourseService
{
    Task<PagedResponse<CourseDto>> ListAsync(
        string? search, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<ServiceResult<CourseDto>> GetAsync(int id, CancellationToken ct = default);

    Task<ServiceResult<CourseDto>> CreateAsync(CourseRequest request, CancellationToken ct = default);

    Task<ServiceResult<CourseDto>> UpdateAsync(int id, CourseRequest request, CancellationToken ct = default);

    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
